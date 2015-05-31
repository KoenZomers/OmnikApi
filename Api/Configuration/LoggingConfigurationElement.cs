using System;
using System.Configuration;
using System.Diagnostics;

namespace KoenZomers.Omnik.Api.Configuration
{
    /// <summary>
    /// Omnik Logging configuration element
    /// </summary>
    public class LoggingConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Log level
        /// </summary>
        [ConfigurationProperty("Level", DefaultValue = TraceLevel.Off, IsRequired = false)]
        public TraceLevel Level
        {
            get { return (TraceLevel)Enum.Parse(typeof(TraceLevel), this["Level"].ToString()); }
            set { this["Level"] = value; }
        }
    }
}
