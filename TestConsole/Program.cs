using System;
using KoenZomers.Omnik.Api;
using System.Net;
using System.Configuration;

namespace KoenZomers.Omnik.TestConsole
{
    /// <summary>
    /// Sample application how to use the Omnik Solar Inverter API
    /// </summary>
    class Program
    {
        /// <summary>
        /// The controller instance used to communicate with the Omnik Solar Inverter
        /// </summary>
        private static Controller _controller;

        /// <summary>
        /// Executed when the program starts
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
            // Create a new instance of the controller to communicate with the Omnik API
            _controller = new Controller();

            // Attach an event handler to the event where statistics have been received from the Omnik Solar Inverter
            _controller.OmnikStatisticsAvailable += OmnikStatisticsAvailable;

            // Start a push session where we open up a TCP port and allow the Omnik to send its statistics in an approximate 5 minute interval to this application
            // Configure the TCP port(s) to listen on via the App.config file
            StartPushData();

            // Start a pull session where we actively connect to the Omnik and query it for its statistics
            StartPullData();

            // Wait for a key to be pressed. As everything is asynchronous and works with events, this can be your normal application operation at this point.
            Console.ReadKey();

            // Stop all possible listeners to clean up
            _controller.StopListeners();
        }

        /// <summary>
        /// Initiates pulling the data out of the Omnik Solar Inverter
        /// </summary>
        private static void StartPullData()
        {                   
            // Request a method to be triggered when raw data is received as a response to the pull action. This is interesting for debugging purposes only.
            _controller.RawPullDataReceived += PullDataReceived;

            // Initiate the pull from the Omnik. Provide its IP address here and its Wifi serial number as that is used as a form of authentication to get the data.
            _controller.PullData(ConfigurationManager.AppSettings["OmnikSolarAddress"], ConfigurationManager.AppSettings["OmnikSolarWiFiSerialNumber"], int.Parse(ConfigurationManager.AppSettings["OmnikSolarPort"]));
        }

        /// <summary>
        /// Initiates opening a TCP port and listening for the data sent to us by the Omnik Solar Inverter
        /// </summary>
        private static void StartPushData()
        {
            // Request a method to be triggered when raw data is received from the Omnik. This is interesting for debugging purposes only. 
            _controller.RawPushDataReceived += PushDataReceived;
            
            // Request a method to be triggered when a TCP listener for the Omnik data has been properly set up and is starting to listen for incoming data
            _controller.Listening += Listening;

            // Request a method to be triggered when a client has connected to our listener
            _controller.ClientConnected += ClientConnected;

            // Request a method to be triggered when a client has disconnected from our listener
            _controller.ClientDisconnected += ClientDisconnected;

            // Start the TCP listener(s) as configured in the App.config file
            _controller.StartListeners();
        }

        /// <summary>
        /// Triggered when statistics have become available in response to a data push or pull action
        /// </summary>
        /// <param name="statistics">Statistics instance with all the information parsed from the information retrieved from the Omnik</param>
        static void OmnikStatisticsAvailable(Statistics statistics)
        {
            Console.WriteLine("Statistics ready @ {0:dddd d MMMM yyyy HH:mm:ss}", DateTime.Now);
            Console.WriteLine("Wifi Module Serial number: {0}", statistics.WifiModuleSerialNumber);
            Console.WriteLine("Inverter Serial number: {0}", statistics.InverterSerialNumber);
            Console.WriteLine("Inverter main firmware version: {0}", statistics.MainFirmwareVersion);
            Console.WriteLine("Inverter slave firmware version: {0}", statistics.SlaveFirmwareVersion);
            Console.WriteLine("Temperature: {0}C", statistics.Temperature);
            Console.WriteLine("Hours active since last reset: {0} hours", statistics.HoursActive);            
            Console.WriteLine("Todays production: {0} kWh", statistics.ProductionToday);
            Console.WriteLine("Total production: {0} kWh", statistics.ProductionTotal);
            Console.WriteLine("Current production #1: {0} Watts", statistics.ProductionCurrent1);
            Console.WriteLine("DC Input #1: {0} volt", statistics.PVVoltageDC1);
            Console.WriteLine("DC Input #1: {0} Amps", statistics.IVAmpsDC1);
            Console.WriteLine("AC Output #1: {0} volt", statistics.PVVoltageAC1);
            Console.WriteLine("AC Output #1: {0} Amps", statistics.IVAmpsAC1);
            Console.WriteLine("Frequency AC: {0} Hz", statistics.FrequencyAC);
        }

        /// <summary>
        /// Triggered when a client disconnects from a listener
        /// </summary>
        /// <param name="endPoint">The remote endpoint that was connected</param>
        /// <param name="listener">The listener the client was connected to</param>
        static void ClientDisconnected(IPEndPoint endPoint, Listener listener)
        {
            Console.WriteLine("Client at {0} has disconnected from listener {1} listening at port {2}", endPoint, listener.Name, listener.PortNumber);
        }

        /// <summary>
        /// Triggered when a client connects to one of our listeners
        /// </summary>
        /// <param name="client">The client instance that has connected</param>
        static void ClientConnected(ConnectedClient client)
        {
            Console.WriteLine("Client connected to listener {0} from {1}", client.Listener.Name, client.RemoteClient);
        }

        /// <summary>
        /// Triggered when a listener is ready and listening
        /// </summary>
        /// <param name="listener">The listener instance that is ready and listening</param>
        static void Listening(Listener listener)
        {
            Console.WriteLine("Listener {0} ready and listening at port {1}", listener.Name, listener.PortNumber);
        }

        /// <summary>
        /// Triggered when raw data is received as a result from a push operation. Useful for debugging purposes only.
        /// </summary>
        /// <param name="receivedData">The byte array with the received data from the Omnik</param>
        /// <param name="client">The client session through which the data has been received</param>
        static void PushDataReceived(byte[] receivedData, ConnectedClient client)
        {
            Console.WriteLine("{0:HH:mm:ss} - Incoming data at listener {1} from {2}. {3} bytes received.", DateTime.Now, client.Listener.Name, client.RemoteClient, receivedData.Length);
        }

        /// <summary>
        /// Triggered when raw data is received as a response to a pull operation. Useful for debugging purposes only.
        /// </summary>
        /// <param name="receivedData">The byte array with the received data from the Omnik</param>
        /// <param name="session">The session instance through which the data has been received</param>
        static void PullDataReceived(byte[] receivedData, DataPullSession session)
        {
            Console.WriteLine("{0:HH:mm:ss} - Incoming data from pull action to {1}:{2}. {3} bytes received.", DateTime.Now, session.OmnikAddress, session.OmnikPort, receivedData.Length);
        }
    }
}
