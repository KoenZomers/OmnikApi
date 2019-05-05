using System.Configuration;

namespace KoenZomers.Omnik.Api.Configuration
{
    /// <summary>
    /// Configuration section definition for Omnik
    /// </summary>
    public class OmnikConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Defines the name of the configuration element used for this section in the app.config file
        /// </summary>
        public const string ConfigurationSectionName = "KoenZomers.Omnik.Configuration";

        [ConfigurationProperty("Listeners", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ListenerConfigurationElementCollection), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public ListenerConfigurationElementCollection Listeners
        {
            get { return (ListenerConfigurationElementCollection)this["Listeners"]; }
            set { this["Listeners"] = value; }
        }

        /// <summary>
        /// The logging configuration parameters
        /// </summary>
        [ConfigurationProperty("Logging")]
        public LoggingConfigurationElement Logging
        {
            get { return (LoggingConfigurationElement)this["Logging"]; }
            set { this["Logging"] = value; }
        }
    }
}
