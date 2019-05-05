using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace KoenZomers.Omnik.Api
{
    /// <summary>
    /// One connected client to a listener
    /// </summary>
    public class ConnectedClient : IDisposable
    {
        #region Constants

        /// <summary>
        /// The default data buffer size to use to retrieve data
        /// </summary>
        private const ushort DefaultDataBufferLength = 1024;

        #endregion

        #region Properties

        private IPEndPoint _remoteClient;
        /// <summary>
        /// Gets the IPEndPoint of the client that is connected or NULL if no client is connected
        /// </summary>
        public IPEndPoint RemoteClient
        {
            get { return _remoteClient; }
        }

        /// <summary>
        /// Gets the listener that accepted this client
        /// </summary>
        public Listener Listener { get; protected set; }

        #endregion

        #region Events

        /// <summary>
        /// Signature for the received data
        /// </summary>
        /// <param name="receivedData">Byte array with the received data</param>
        /// <param name="client">The client instance on which the data has been received</param>
        public delegate void DataReceivedHandler(byte[] receivedData, ConnectedClient client);

        /// <summary>
        /// Triggered when data is received from the client
        /// </summary>
        public event DataReceivedHandler DataReceived;

        /// <summary>
        /// Signature for a disconnecting client 
        /// </summary>
        /// <param name="client">Client instance that has disconnected</param>
        public delegate void ClientDisconnectedHandler(ConnectedClient client);

        /// <summary>
        /// Triggered when the remote client connected to this instance disconnects
        /// </summary>
        public event ClientDisconnectedHandler ClientDisconnected;

        #endregion

        #region Fields

        /// <summary>
        /// The client that this handler represents
        /// </summary>
        private TcpClient _tcpClient { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Set up a new TcpClientHandler instance originating from a new client connecting to the service
        /// </summary>
        /// <param name="client">TcpClient of the new incoming client connection</param>
        /// <param name="listener">The Listener on which the client has connected</param>
        public ConnectedClient(TcpClient client, Listener listener)
        {
            _tcpClient = client;
            Listener = listener;
            _remoteClient = _tcpClient.Client.RemoteEndPoint as IPEndPoint;

            // Wait for data to be received
            var dataBuffer = new byte[DefaultDataBufferLength];
            _tcpClient.GetStream().BeginRead(dataBuffer, 0, dataBuffer.Length, HandleReceivedData, dataBuffer);
        }

        #endregion

        /// <summary>
        /// Handles data received from the connected client
        /// </summary>
        private void HandleReceivedData(IAsyncResult ar)
        {
            // Check that the stream is still open to read data from
            if (ar == null) return;
            if (!_tcpClient.GetStream().CanRead) return;

            // Receive the amount of received data in bytes            
            var receivedDataCountInBytes = _tcpClient.GetStream().EndRead(ar);

            // Check if data has been received or the client has disconnected
            if (receivedDataCountInBytes == 0)
            {
                Disconnect();
                return;
            }

            // Retrieve the data buffer
            var dataBuffer = (byte[])ar.AsyncState;

            // Copy the actual received data from the buffer
            var receivedData = new byte[receivedDataCountInBytes];
            Buffer.BlockCopy(dataBuffer, 0, receivedData, 0, receivedDataCountInBytes);

            var receivedDataHexadecimal = BitConverter.ToString(receivedData);
            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Data received at Onmnik Listener client device at {0}: '{1}' (length {2})", _tcpClient.Client.RemoteEndPoint, receivedDataHexadecimal, receivedDataHexadecimal.Length));

            /// Check if there are event handlers attached to signal of the data that has been received
            if (DataReceived != null)
            {
                DataReceived(receivedData, this);
            }

            // Reinitialize the client to retrieve new data
            _tcpClient.GetStream().BeginRead(dataBuffer, 0, dataBuffer.Length, HandleReceivedData, dataBuffer);
        }

        /// <summary>
        /// Disconnects the client
        /// </summary>
        public void Disconnect()
        {
            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Disconnecting from Omnik Listener Client at {0}", RemoteClient));

            // Check if there are subscribers to the event for disconnecting clients
            if (ClientDisconnected != null)
            {
                ClientDisconnected(this);
            }

            Dispose();

            Logging.Logdata.LogMessage(TraceLevel.Info, string.Format("Disconnected from Omnik Listener Client at {0}", RemoteClient));
        }

        /// <summary>
        /// Clean up this client instance
        /// </summary>
        public void Dispose()
        {
            if (_tcpClient == null) return;
            try
            {
                var stream = _tcpClient.GetStream();                
                if (stream != null) stream.Close();
            }
            catch (ObjectDisposedException)
            {
                // Ignore as the instance is already disposed which is good
            }
            _tcpClient.Close();
            _tcpClient = null;
        }
    }
}