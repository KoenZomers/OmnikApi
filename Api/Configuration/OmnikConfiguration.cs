using System.Configuration;
using KoenZomers.Omnik.Api.Exceptions;

namespace KoenZomers.Omnik.Api.Configuration
{
    /// <summary>
    /// Static class which provides access to the Omnik Configuration items in the application configuration file
    /// </summary>
    public static class OmnikConfiguration
    {
        private static OmnikConfigurationSection _instance;
        /// <summary>
        /// Gets an instance to the Omnik configuration items in the application configuration file
        /// </summary>
        public static OmnikConfigurationSection Instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        _instance = (OmnikConfigurationSection)ConfigurationManager.GetSection(OmnikConfigurationSection.ConfigurationSectionName);
                    }
                    catch (ConfigurationErrorsException ex)
                    {
                        throw new Exceptions.ConfigurationException(string.Concat("Could not parse configuration for Omnik Api. '", OmnikConfiguration.Instance, "' configuration element not found in application configuration file."), ex);
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Refreshes the configuration from the application configuration file
        /// </summary>
        public static void RefreshConfiguration()
        {
            _instance = null;
            ConfigurationManager.RefreshSection(OmnikConfigurationSection.ConfigurationSectionName);
        }
    }
}
