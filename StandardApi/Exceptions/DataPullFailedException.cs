using System;
using System.Net;
using System.Runtime.Serialization;

namespace KoenZomers.Omnik.Api.Exceptions
{
    /// <summary>
    /// Exception thrown when a data pull operation fails
    /// </summary>
    [Serializable]
    public class DataPullFailedException : Exception
    {
        #region Properties

        /// <summary>
        /// The IP EndPoint to which a connection attempt was made
        /// </summary>
        public IPEndPoint IPEndPoint { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor because its required for serialization support. Do not use.
        /// </summary>
        public DataPullFailedException()
        {
        }

        /// <summary>
        /// Throw a new exception passing the specifics to aid in troubleshooting
        /// </summary>
        /// <param name="ipEndPoint">IPEndPoint to which the connection attempt was made</param>
        /// <param name="message">Message describing the problem</param>
        public DataPullFailedException(IPEndPoint ipEndPoint, string message) : base("Could not connect to " + ipEndPoint.ToString() + " because " + message)
        {
            IPEndPoint = ipEndPoint;
        }

        /// <summary>
        /// Throw a new exception passing the specifics to aid in troubleshooting
        /// </summary>
        /// <param name="ipEndPoint">IPEndPoint to which the connection attempt was made</param>
        /// <param name="innerException">Inner exception raised when trying to connect</param>
        public DataPullFailedException(IPEndPoint ipEndPoint, Exception innerException) : base("Could not connect to " + ipEndPoint.ToString() + " because " + innerException.Message, innerException)
        {
            IPEndPoint = ipEndPoint;
        }

        #endregion

        #region Serialization support

        protected DataPullFailedException(SerializationInfo info, StreamingContext context): base(info, context)
		{
			if (info != null)
			{
                IPEndPoint = (IPEndPoint) info.GetValue("ipEndPoint", IPEndPoint.GetType());
			}
		}

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

            info.AddValue("ipEndPoint", IPEndPoint);
        }

        #endregion
    }
}
