using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace KoenZomers.Omnik.Api
{
    /// <summary>
    /// One listener listening for incoming data from an Omnik Solar Inverter
    /// </summary>
    public class Listener : IDisposable
    {
        #region Constants

        /// <summary>
        /// The default data buffer size to use to retrieve data
        /// </summary>
        private const ushort DefaultDataBufferLength = 1024;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the TCP port number at which this listener listens for Omnik data transmissions
        /// </summary>
        public int PortNumber { get; protected set; }

        /// <summary>
        /// Gets a boolean indicating if the listener is currently listening for incoming data
        /// </summary>
        public bool IsListening { get; protected set; }

        /// <summary>
        /// Gets the name assigned to this listener
        /// </summary>
        public string Name { get; protected set; }

        #endregion

        #region Fields

        /// <summary>
        /// The TcpListener instance used to receive data from the Omnik
        /// </summary>
        protected TcpListener tcpListener;

        /// <summary>
        /// List with all connected clients
        /// </summary>
        private List<ConnectedClient> connectedClients = new List<ConnectedClient>();

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
        /// Signature for the received data
        /// </summary>
        /// <param name="receivedData">Byte array with the received data</param>
        /// <param name="client">The client instance on which the data has been received</param>
        public delegate void DataReceivedHandler(byte[] receivedData, ConnectedClient client);

        /// <summary>
        /// Triggered when data is received from a client
        /// </summary>
        public event DataReceivedHandler DataReceived;

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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new Omnik Listener instance with the provided name on the provided port number
        /// </summary>
        /// <param name="portNumber">Number of the TCP port to listen on for incoming Omnik data</param>
        /// <param name="name">Name to assign to this listener</param>
        public Listener(int portNumber, string name) : this(portNumber)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new Omnik Listener instance on the provided port number
        /// </summary>
        /// <param name="portNumber">Number of the TCP port to listen on for incoming Omnik data</param>
        public Listener(int portNumber)
        {
            if(portNumber < 1 || portNumber > 65535)
            {
                Logging.Logdata.LogMessage(TraceLevel.Error, string.Format("Trying to create a listener on invalid port number {0}", portNumber));
                throw new ArgumentOutOfRangeException("portNumber", "Port number should be in the range 1 - 65535");
            }

            PortNumber = portNumber;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Opens the TCP port to start listening for incoming data transmissions from an Omnik device
        /// </summary>
        public void Start()
        {
            // Check if there's already a listener open and if so, close it
            if (tcpListener != null)
            {
                Logging.Logdata.LogMessage(TraceLevel.Warning, string.Format("Listener was already active when attempting to start it. Closing listener named {0} to reopen at port TCP {1}.", Name, PortNumber));
                tcpListener.Stop();
            }

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Starting listener at TCP {0} named {1}", PortNumber, Name));

            try
            {
                // Set up the TCP listener
                tcpListener = new TcpListener(IPAddress.Any, PortNumber);
                tcpListener.Start();
            }
            catch (Exception ex)
            {
                Logging.Logdata.LogMessage(TraceLevel.Error, string.Format("Unable to start listening at TCP {0} using listener named {1} because {2}", PortNumber, Name, ex.Message));
                throw new Exceptions.ListenerException(PortNumber, ex);
            }

            Logging.Logdata.LogMessage(TraceLevel.Info, string.Format("Listening at TCP {0} using listener named {1}", PortNumber, Name));
            IsListening = true;

            // Wait for a client to connect
            tcpListener.BeginAcceptTcpClient(HandleClientConnecting, tcpListener);

            // Check if there are subscribers to the Listener ready event to signal
            if(Listening != null)
            {
                Listening(this);
            }
        }

        /// <summary>
        /// Triggered when a client connects
        /// </summary>
        private void HandleClientConnecting(IAsyncResult ar)
        {
            var tcpListener = (TcpListener)ar.AsyncState;
            
            // Check if a connection has been made. This isn't the case when shutting down.
            if (!tcpListener.Server.Connected) return;

            TcpClient client;
            try
            {
                client = tcpListener.EndAcceptTcpClient(ar);
            }
            catch (ObjectDisposedException)
            {
                // When stopping the application hosting this Api, the waiting listener may throw an ObjectDisposedException. This is fine to be ignored here.
                return;
            }

            // Set the LingerState so that connection closes occur immediately instead of waiting for data to still be sent as this seems to have a negative impact on connections being left open
            // See http://msdn.microsoft.com/en-us/library/system.net.sockets.tcpclient.lingerstate%28v=vs.110%29.aspx for options
            client.LingerState = new LingerOption(true, 0);

            // Create a new client handler instance to handle the client requests
            var handler = new ConnectedClient(client, this);
            handler.DataReceived += HandleReceivedData;
            handler.ClientDisconnected += ClientHasDisconnected;

            // Check if there are subscribers to the ClientConnected event to signal
            if (ClientConnected != null)
            {
                ClientConnected(handler);
            }

            // Register the client so we can keep track of it
            connectedClients.Add(handler);

            // Start listening for another client to connect
            tcpListener.BeginAcceptTcpClient(HandleClientConnecting, tcpListener);
        }

        /// <summary>
        /// Triggered when data is received from a connected client
        /// </summary>
        /// <param name="receivedData">Byte array with the received data</param>
        /// <param name="client">The client instance on which the data has been received</param>
        private void HandleReceivedData(byte[] receivedData, ConnectedClient client)
        {
            // Check if there are subscribers to our event and if so, signal them of the data that has been received
            if(DataReceived != null)
            {
                DataReceived(receivedData, client);
            }
        }

        /// <summary>
        /// Triggered when a client has disconnected
        /// </summary>
        /// <param name="client">Client instance that has disconnected</param>
        private void ClientHasDisconnected(ConnectedClient client)
        {
            // Check if there are subscribers to the client disconnecting event to signal
            if (ClientDisconnected != null)
            {
                ClientDisconnected(client.RemoteClient, this);
            }

            connectedClients.Remove(client);
            client.Dispose();
        }

        /// <summary>
        /// Stops listening
        /// </summary>
        public void Stop()
        {
            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Closing listener at TCP {0} named {1}", PortNumber, Name));

            // Check if the listener was active already
            if (tcpListener == null) return;

            // Disconnect all open clients
            while(connectedClients.Count > 0)
            {
                ClientHasDisconnected(connectedClients[0]);
            }

            // Stop the listener
            tcpListener.Stop();
            tcpListener = null;

            Logging.Logdata.LogMessage(TraceLevel.Info, string.Format("Listener at TCP {0} named {1} has been closed", PortNumber, Name));
        } 

        /// <summary>
        /// Closes the connection and cleans up used resources
        /// </summary>
        void IDisposable.Dispose()
        {
            Stop();
        }

        #endregion
    }
}
