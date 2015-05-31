using System;
using System.Runtime.Serialization;

namespace KoenZomers.Omnik.Api.Exceptions
{
    /// <summary>
    /// Exception thrown when there are problems with opening the listener
    /// </summary>
    [Serializable]
    public class ListenerException : Exception
    {
        #region Properties

        /// <summary>
        /// The port number at which the listener tried to listen on
        /// </summary>
        public int PortNumber { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor because its required for serialization support. Do not use.
        /// </summary>
        public ListenerException()
        {
        }

        /// <summary>
        /// Throw a new exception passing the specifics to aid in troubleshooting
        /// </summary>
        /// <param name="portNumber">Portnumber at which the listener tried to listen</param>
        /// <param name="message">Message describing the problem</param>
        public ListenerException(int portNumber, string message) : base("Could not listen at TCP " + portNumber + " because " + message)
        {
            PortNumber = portNumber;
        }

        /// <summary>
        /// Throw a new exception passing the specifics to aid in troubleshooting
        /// </summary>
        /// <param name="portNumber">Portnumber at which the listener tried to listen</param>
        /// <param name="innerException">Inner exception raised when trying to connect</param>
        public ListenerException(int portNumber, Exception innerException) : base("Could not listen at TCP " + portNumber + " because " + innerException.Message, innerException)
        {
            PortNumber = portNumber;
        }

        #endregion

        #region Serialization support

        protected ListenerException(SerializationInfo info, StreamingContext context): base(info, context)
		{
			if (info != null)
			{
			    PortNumber = info.GetUInt16("portnumber");
			}
		}

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

            info.AddValue("portnumber", PortNumber);
        }

        #endregion
    }
}
