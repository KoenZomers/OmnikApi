using System.Configuration;

namespace KoenZomers.Omnik.Api.Configuration
{
    /// <summary>
    /// Omnik Listener definition
    /// </summary>
    public class ListenerConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Name to assign to this listener
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = "", IsRequired = true)]
        //[StringValidator(MinLength = 1)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Port number on which should be listened for Omnik data transmissions
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = 8844, IsRequired = true)]
        //[StringValidator(MinLength = 1)]
        [IntegerValidator(MinValue = 1, MaxValue = 65535)]
        public int Port
        {
            get 
            {
                int port;
                return int.TryParse(this["port"].ToString(), out port) ? port : 0;
            }
            set { this["port"] = value; }
        }
    }
}
