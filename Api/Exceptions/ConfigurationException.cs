using System;

namespace KoenZomers.Omnik.Api.Exceptions
{
    /// <summary>
    /// Exception thrown when there are problems with the Omnik configuration element in the application configuration file
    /// </summary>
    [Serializable]
    public class ConfigurationException : Exception
    {
        public ConfigurationException()
        {
        }

        public ConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
