using System;
using System.Globalization;

namespace KoenZomers.Omnik.Api
{
    /// <summary>
    /// Contains utilities for handling the data in this project
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Converts a decimal value to its hexadecimal representation
        /// </summary>
        /// <param name="value">Decimal number</param>
        /// <returns>Hexidecimal equivallent of the decimal number</returns>
        public static string ConvertDecimalToHex(int value)
        {
            return value.ToString("x");
        }

        /// <summary>
        /// Converts a string value with decimal numbers to its hexadecimal representation
        /// </summary>
        /// <param name="value">String containing decimal numbers</param>
        /// <returns>Hexadecimal equivallent string of the decimal numbers</returns>
        public static string ConvertStringToHex(string value)
        {
            var number = int.Parse(value);
            return number.ToString("x");
        }

        /// <summary>
        /// Converts a hexidecimal string to its byte array equivallent
        /// </summary>
        /// <param name="hex">Hexadecimal string</param>
        /// <returns>Byte Array</returns>
        public static byte[] ConvertHexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hex));
            }

            byte[] HexAsBytes = new byte[hex.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hex.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return HexAsBytes;
        }
    }
}
