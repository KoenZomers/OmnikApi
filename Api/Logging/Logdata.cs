using System.Diagnostics;

namespace KoenZomers.Omnik.Api.Logging
{
    /// <summary>
    /// Class which takes care of logging data to the configured Trace Writers
    /// </summary>
    public class Logdata
    {
        #region Properties

        /// <summary>
        /// Name to use to identify messages logged by this class
        /// </summary>
        public static readonly string LogCategory = "Omnik Api";

        #endregion

        #region Public methods

        /// <summary>
        /// Logs the message if the application is configured to log messages of the provided level
        /// </summary>
        /// <param name="level">Logging level this message concerns</param>
        /// <param name="message">Message to be logged</param>
        public static void LogMessage(TraceLevel level, string message)
        {
            // Check if the message should be logged according to the current application configuration
            var shouldThisMessageBeLogged = ShouldThisMessageBeLogged(level);

            // Only continue if the message should be logged
            if (!shouldThisMessageBeLogged) return;

            // Log the message
            Trace.WriteLine(string.Format("{0},{1},{2}", LogCategory, level, message));
        }

        /// <summary>
        /// Always logs the message, regardless of the configured level to log
        /// </summary>
        /// <param name="level">Logging level this message concerns</param>
        /// <param name="message">Message to be logged</param>
        public static void AlwaysLogMessage(TraceLevel level, string message)
        {
            // Log the message
            Trace.WriteLine(string.Format("{0},{1},{2}", LogCategory, level, message));
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Returns a boolean indicating if a message in the provided category for the provided logging level should be logged based on the logging configuration of this application
        /// </summary>
        /// <param name="level">Level in which a message is to be logged</param>
        /// <returns>True if the message should be logged, false if not</returns>
        private static bool ShouldThisMessageBeLogged(TraceLevel level)
        {
            if (Configuration.OmnikConfiguration.Instance == null) return true;

            var logThisMessage = Configuration.OmnikConfiguration.Instance.Logging.Level >= level;
            return logThisMessage;
        }

        #endregion
    }
}
