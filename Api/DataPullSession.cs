using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace KoenZomers.Omnik.Api
{
    /// <summary>
    /// Contains the logic for pulling statistics out of the Omnik Solar Inverter
    /// </summary>
    public class DataPullSession
    {
        #region Constants

        /// <summary>
        /// The default port number of the Omnik to which a data pull session will be made
        /// </summary>
        private const int DefaultOmnikPortNumber = 8899;

        /// <summary>
        /// The default data buffer size to use to retrieve data
        /// </summary>
        private const ushort DefaultDataBufferLength = 1024;

        #endregion
        
        #region Properties

        /// <summary>
        /// Gets the Wifi serialnumber used in this datapull session
        /// </summary>
        public string WifiSerialNumber { get; protected set; }

        /// <summary>
        /// Gets the IP EndPoint of the Omnik from which data will be retrieved
        /// </summary>
        public IPEndPoint IPEndPoint { get; protected set;}

        #endregion

        #region Fields

        /// <summary>
        /// The TcpClient used to connect to the Omnik
        /// </summary>
        private TcpClient _tcpClient { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Signature for the received data
        /// </summary>
        /// <param name="receivedData">Byte array with the received data</param>
        /// <param name="dataPullSession">The data pull session through which data has been received</param>
        public delegate void DataReceivedHandler(byte[] receivedData, DataPullSession dataPullSession);

        /// <summary>
        /// Triggered when data is received from the Omnik
        /// </summary>
        public event DataReceivedHandler DataReceived;

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

        #region Constructors

        /// <summary>
        /// Creates a new data pull session to the Omnik with the provided IP address using the default port number and the provided serial number
        /// </summary>
        /// <param name="ipAddress">IP Address of the Omnik</param>
        /// <param name="serialNumber">Serial number of the Omnik</param>
        public DataPullSession(IPAddress ipAddress, string serialNumber) : this(ipAddress, DefaultOmnikPortNumber, serialNumber)
        {
        }

        /// <summary>
        /// Creates a new data pull session to the Omnik with the provided IP address on the provided port number using the provided serial number
        /// </summary>        
        /// <param name="ipAddress">IP Address of the Omnik</param>
        /// <param name="portNumber">Port number on the Omnik to connect to</param>
        /// <param name="wifiSerialNumber">Serial number of the Wifi module in the Omnik</param>
        public DataPullSession(IPAddress ipAddress, int portNumber, string wifiSerialNumber)
        {
            WifiSerialNumber = wifiSerialNumber;
            IPEndPoint = new IPEndPoint(ipAddress, portNumber);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("New data pull session to {0} with wifi serialnumber {1} has been initialized", IPEndPoint, WifiSerialNumber));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Retrieve data from the Omnik
        /// </summary>
        public void RetrieveData()
        {
            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Connecting to Omnik at {0} with serialnumber {1} to pull data", IPEndPoint, WifiSerialNumber));

            // Initiate the connection to the Omnik asynchronously
            _tcpClient = new TcpClient(AddressFamily.InterNetwork);
            _tcpClient.BeginConnect(IPEndPoint.Address, IPEndPoint.Port, HandlePullClientConnected, _tcpClient);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Handles a pull client being connected to an Omnik
        /// </summary>
        private void HandlePullClientConnected(IAsyncResult ar)
        {
            var tcpClient = (TcpClient)ar.AsyncState;
            
            if (!tcpClient.Connected)
            {
                var errorMessage = string.Format("Unable to connect to Omnik at {0} with serialnumber {1} to pull data", IPEndPoint, WifiSerialNumber);
                Logging.Logdata.LogMessage(TraceLevel.Verbose, errorMessage);
                
                if(DataPullSessionFailed != null)
                {
                    DataPullSessionFailed(IPEndPoint.Address, WifiSerialNumber, errorMessage);
                }
                return;
            }

            tcpClient.EndConnect(ar);
            
            // Send out the message to the Omnik to request the statistics
            SendDataPullMessage(tcpClient);

            // Wait for data to be received in response to sending out the data request
            var dataBuffer = new byte[DefaultDataBufferLength];
            try
            {
                _tcpClient.GetStream().BeginRead(dataBuffer, 0, dataBuffer.Length, HandleReceivedData, dataBuffer);
            }
            catch (Exception exception)
            {
                Logging.Logdata.LogMessage(TraceLevel.Warning, string.Format("Error while waiting for reply from Omnik at {0} with serialnumber {1} in response to the initial statistics request. Exception: {2}. StackTrace: {3}.", IPEndPoint, WifiSerialNumber, exception.Message, exception.StackTrace));
            }
            
        }

        /// <summary>
        /// Constructs the message to send to the Omnik to request data and sends it
        /// </summary>
        /// <param name="tcpClient">TcpClient to use to transmit the message</param>
        private void SendDataPullMessage(TcpClient tcpClient)
        {
            if (!tcpClient.Connected)
            {
                Logging.Logdata.LogMessage(TraceLevel.Warning, string.Format("Can't send to Omnik at {0} with serialnumber {1} because the connection is closed", IPEndPoint, WifiSerialNumber));
                return;
            }

            // Convert the serial number into a bytes array
            var serialHex = Utils.ConvertStringToHex(WifiSerialNumber);
            var serialBytes = Utils.ConvertHexStringToByteArray(serialHex);

            // Calculate the checksum
            var checksum = 0;
            for (var x = 0; x < 4; x++)
            {
                checksum += serialBytes[x];
            }
            checksum *= 2;
            checksum += 115;
            checksum &= 0xff;

            // Convert the checksum into a byte
            var checksumHex = Utils.ConvertDecimalToHex(checksum);
            var checksumBytes = Utils.ConvertHexStringToByteArray(checksumHex);

            // Construct the message which requests the statistics
            var requestDataMessage = new byte[] { 0x68, 0x02, 0x40, 0x30, serialBytes[3], serialBytes[2], serialBytes[1], serialBytes[0], serialBytes[3], serialBytes[2], serialBytes[1], serialBytes[0], 0x01, 0x00, checksumBytes[0], 0x16 };

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Sending Omnik at {0} with serialnumber {1} the request statistics command {2}", IPEndPoint, WifiSerialNumber, BitConverter.ToString(requestDataMessage)));

            // Send the message to the Omnik
            tcpClient.GetStream().Write(requestDataMessage, 0, requestDataMessage.Length);
        }

        /// <summary>
        /// Handles data received from the connected session
        /// </summary>
        private void HandleReceivedData(IAsyncResult ar)
        {
            int receivedDataCountInBytes;
            try
            {
                // Get the networkstream from the TcpClient
                var stream = _tcpClient.GetStream();

                // Check that the stream is still open to read data from
                if (!stream.CanRead) return;

                // Receive the amount of received data in bytes            
                receivedDataCountInBytes = stream.EndRead(ar);
            }
            catch (Exception exception)
            {
                Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Onmnik at {0} disconnected while delivering data. Exception: {1}. Error message: {2}.", IPEndPoint, exception.Message, exception.StackTrace));
                return;
            }
            
            // Check if data has been received or the client has disconnected
            if (receivedDataCountInBytes == 0)
            {
                //Disconnect();
                return;
            }

            // Retrieve the data buffer
            var dataBuffer = (byte[])ar.AsyncState;

            // Copy the actual received data from the buffer
            var receivedData = new byte[receivedDataCountInBytes];
            Buffer.BlockCopy(dataBuffer, 0, receivedData, 0, receivedDataCountInBytes);

            // Check if data has been received or the client has disconnected
            if (receivedDataCountInBytes == 0)
            {
                Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Onmnik at {0} disconnected after data pull request", IPEndPoint));

                _tcpClient.Close();
                return;
            }

            var receivedDataHexadecimal = BitConverter.ToString(receivedData);
            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Data received from Onmnik at {0} after data pull request: '{1}' (length {2})", IPEndPoint, receivedDataHexadecimal, receivedDataHexadecimal.Length));

            // Check if there are event handlers attached to signal of the data that has been received
            if (DataReceived != null)
            {
                DataReceived(receivedData, this);
            }

            // Reinitialize the client to retrieve new data
            _tcpClient.GetStream().BeginRead(dataBuffer, 0, dataBuffer.Length, HandleReceivedData, dataBuffer);
        }

        #endregion

    }
}
