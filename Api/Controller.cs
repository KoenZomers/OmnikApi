using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace KoenZomers.Omnik.Api
{
    /// <summary>
    /// Controls the communication to and from the Omnik Solar Inverters
    /// </summary>
    public class Controller
    {
        #region Properties

        /// <summary>
        /// List with all listening listeners
        /// </summary>
        public List<Listener> Listeners = new List<Listener>();

        #endregion

        #region Fields

        #endregion

        #region Events

        /// <summary>
        /// Signature for listener ready
        /// </summary>
        /// <param name="listener">Listener instance that is ready and listening for incoming data</param>
        public delegate void ListeningHandler(Listener listener);
        /// <summary>
        /// Triggered when a listener is ready and listening
        /// </summary>
        public event ListeningHandler Listening;

        /// <summary>
        /// Signature for the received data pushed towards us from an Omin
        /// </summary>
        /// <param name="receivedData">Byte array with the received data</param>
        /// <param name="client">The client instance on which the data has been received</param>
        public delegate void RawPushDataReceivedHandler(byte[] receivedData, ConnectedClient client);

        /// <summary>
        /// Triggered when data is received from an Omnik pushing the data to us
        /// </summary>
        public event RawPushDataReceivedHandler RawPushDataReceived;

        /// <summary>
        /// Signature for the received data pulled from an Omnik
        /// </summary>
        /// <param name="receivedData">Byte array with the received data</param>
        /// <param name="session">The session which has pulled the data from the Omnik</param>
        public delegate void RawPullDataReceivedHandler(byte[] receivedData, DataPullSession session);

        /// <summary>
        /// Triggered when data is received from an Omnik by pulling it from the Omnik
        /// </summary>
        public event RawPullDataReceivedHandler RawPullDataReceived;

        /// <summary>
        /// Signature for a connecting client
        /// </summary>
        /// <param name="client">Client instance representing the client that connects</param>
        public delegate void ClientConnectedHandler(ConnectedClient client);
        /// <summary>
        /// Triggered when a client connects
        /// </summary>
        public event ClientConnectedHandler ClientConnected;

        /// <summary>
        /// Signature for a disconnecting client
        /// </summary>
        /// <param name="clientEndPoint">EndPoint of the client that disconnected</param>
        /// <param name="listener">The Listener from which the client has disconnected</param>
        public delegate void ClientDisconnectedHandler(IPEndPoint clientEndPoint, Listener listener);
        /// <summary>
        /// Triggered when a client disconnects
        /// </summary>
        public event ClientDisconnectedHandler ClientDisconnected;

        /// <summary>
        /// Signature for when Omnik statistics are available
        /// </summary>
        /// <param name="statistics">The statistics parsed from the Omnik data</param>
        public delegate void OmnikStatisticsAvailableHandler(Statistics statistics);
        /// <summary>
        /// Triggered when either a datapull completes or the Omnik has sent data to us
        /// </summary>
        public event OmnikStatisticsAvailableHandler OmnikStatisticsAvailable;

        /// <summary>
        /// Signature for the failed pull session
        /// </summary>
        /// <param name="ipAddress">IP Address of the Omnik</param>
        /// <param name="serialNumber">Serial number of the Omnik</param>
        /// <param name="reason">Reason why the connection failed</param>
        public delegate void DataPullSessionFailedHandler(IPAddress ipAddress, string serialNumber, string reason);

        /// <summary>
        /// Triggered when the pull session failed
        /// </summary>
        public event DataPullSessionFailedHandler DataPullSessionFailed;

        #endregion

        #region Public methods

        /// <summary>
        /// Starts all listeners configured in the application configuration file
        /// </summary>
        public void StartListeners()
        {
            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Creating {0} listener(s) as registered in the application configuration file", Configuration.OmnikConfiguration.Instance.Listeners.Count));

            // Loop through each of the listeners configured in the application configuration file
            foreach (Configuration.ListenerConfigurationElement listenerConfiguration in Configuration.OmnikConfiguration.Instance.Listeners)
            {
                // Create a new listener instance based on the configuration parameters
                Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Creating listener named {0} on TCP port {1}", listenerConfiguration.Name, listenerConfiguration.Port));
                var listener = new Listener(listenerConfiguration.Port, listenerConfiguration.Name);
                
                // Attach event handlers to the listener
                listener.Listening += HandleListening;
                listener.ClientConnected += HandleClientConnected;
                listener.ClientDisconnected += HandleClientDisconnected;
                listener.DataReceived += HandlePushSessionDataReceived;

                // Start the listener
                listener.Start();

                // Add the listener to the collection with listeners
                Listeners.Add(listener);

                Logging.Logdata.LogMessage(TraceLevel.Info, string.Format("Listener named {0} on TCP port {1} has been created", listenerConfiguration.Name, listenerConfiguration.Port));
            }
        }

        /// <summary>
        /// Stops all currently listening listeners
        /// </summary>
        public void StopListeners()
        {
            // Loop through each of the listneers and stop them
            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Stopping all {0} active listener(s)", Listeners.Count));

            foreach(var listener in Listeners)
            {
                listener.Stop();
            }

            Logging.Logdata.LogMessage(TraceLevel.Info, "All active listener(s) have stopped");
        }

        /// <summary>
        /// Pulls data in from an Omnik Solar Converter by connecting to it. Subscribe to the DataPullSessionCompleted event to retrieve the statistics once it completes.
        /// </summary>
        /// <param name="omnikIPAddress">IP Address of the Omnik Solar Inverter to connect to</param>
        /// <param name="serialNumber">Serial number of the Omnik Solar Inverter to which the connection will be made</param>
        public void PullData(IPAddress omnikIPAddress, string serialNumber)
        {
            // Create a new data pull session and initiate it
            var dataPullSession = new DataPullSession(omnikIPAddress, serialNumber);
            dataPullSession.DataReceived += HandlePullSessionDataReceived;
            dataPullSession.DataPullSessionFailed += HandleDataPullSessionFailed;
            dataPullSession.RetrieveData();               
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Triggered when a data pull session failed
        /// </summary>
        /// <param name="ipAddress">IP Address of the Omnik</param>
        /// <param name="serialNumber">Serial number of the Omnik</param>
        /// <param name="reason">Reason why the connection failed</param>
        private void HandleDataPullSessionFailed(IPAddress ipAddress, string serialNumber, string reason)
        {
            if (DataPullSessionFailed != null)
            {
                DataPullSessionFailed(ipAddress, serialNumber, reason);
            }
        }

        /// <summary>
        /// Triggered when data is received based on a pull request
        /// </summary>
        /// <param name="receivedData">Byte array with the received data</param>
        /// <param name="session">The data pull session through which data has been received</param>
        private void HandlePullSessionDataReceived(byte[] receivedData, DataPullSession session)
        {
            // Check if there are subscribers for the raw Omnik data and signal them
            if (RawPullDataReceived != null)
            {
                RawPullDataReceived(receivedData, session);
            }

            // Check if there are subscribers to the completed data pull sessions and signal them. Allow for some variations in the received data length due to different firmware versions using different transmission lengths.
            if (OmnikStatisticsAvailable != null && receivedData.Length >= 130 && receivedData.Length <= 150)
            {
                // Create the statistics
                var statistics = new Statistics(receivedData);
                OmnikStatisticsAvailable(statistics);
            }
        }

        /// <summary>
        /// Triggered when data is received from a client by having the Omnik push it to us
        /// </summary>
        /// <param name="receivedData">Byte array with the received data</param>
        /// <param name="client">The client instance on which the data has been received</param>
        private void HandlePushSessionDataReceived(byte[] receivedData, ConnectedClient client)
        {
            // Check if there are subscribers for the raw Omnik data and signal them
            if (RawPushDataReceived != null)
            {
                RawPushDataReceived(receivedData, client);
            }

            // Check if there are subscribers for the parsed Omnik statistics and that the received data is the expected 139 bytes and signal them
            if (OmnikStatisticsAvailable != null && receivedData.Length == 139)
            {
                // Create the statistics
                var statistics = new Statistics(receivedData);
                OmnikStatisticsAvailable(statistics);
            }
        }

        /// <summary>
        /// Triggered when a client disconnects
        /// </summary>
        /// <param name="clientEndPoint">EndPoint of the client that disconnected</param>
        /// <param name="listener">The Listener from which the client has disconnected</param>
        private void HandleClientDisconnected(System.Net.IPEndPoint clientEndPoint, Listener listener)
        {
            if (ClientConnected != null)
            {
                ClientDisconnected(clientEndPoint, listener);
            }
        }

        /// <summary>
        /// Triggered when a client connects
        /// </summary>
        /// <param name="client">Client instance representing the client that connects</param>
        private void HandleClientConnected(ConnectedClient client)
        {
            if(ClientConnected != null)
            {
                ClientConnected(client);
            }
        }

        /// <summary>
        /// Triggered when a listener is ready and listening
        /// </summary>
        /// <param name="receivedData">Byte array with the received data</param>
        /// <param name="receivedDataAsText">Received data converted to hexadecimals</param>
        /// <param name="client">The client instance on which the data has been received</param>/// 
        private void HandleListening(Listener listener)
        {
            if(Listening != null)
            {
                Listening(listener);
            }
        }

        #endregion
    }
}
