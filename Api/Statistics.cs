using System;
using System.Diagnostics;
using System.Text;

namespace KoenZomers.Omnik.Api
{
    /// <summary>
    /// Contains one set of statistics based on data provided by the Omnik
    /// </summary>
    public class Statistics
    {
        #region Properties

        /// <summary>
        /// Gets the raw data as received from the Omnik
        /// </summary>
        public byte[] RawData { get; protected set; }

        /// <summary>
        /// Gets the serial number of the wifi module
        /// </summary>
        public string WifiModuleSerialNumber { get; protected set; }

        /// <summary>
        /// Gets the serial number of the inverter
        /// </summary>
        public string InverterSerialNumber { get; protected set; }

        /// <summary>
        /// Gets the main firmware version of the inverter
        /// </summary>
        public string MainFirmwareVersion { get; protected set; }

        /// <summary>
        /// Gets the slave firmware version of the inverter
        /// </summary>
        public string SlaveFirmwareVersion { get; protected set; }

        /// <summary>
        /// Gets the temperature
        /// </summary>
        public decimal Temperature { get; protected set; }

        /// <summary>
        /// Gets the amount of hours the Omnik is active already since its last reset
        /// </summary>
        public int HoursActive { get; protected set; }

        /// <summary>
        /// Gets the current electricity production in Watts on channel 1
        /// </summary>
        public int ProductionCurrent1 { get; protected set; }

        /// <summary>
        /// Gets the current electricity production in Watts on channel 2
        /// </summary>
        public int ProductionCurrent2
        {
            get { throw new NotImplementedException("Retrieving the current electricity production on channel 2 is not yet implemented"); }
        }

        /// <summary>
        /// Gets the current electricity production in Watts on channel 3
        /// </summary>
        public int ProductionCurrent3
        {
            get { throw new NotImplementedException("Retrieving the current electricity production on channel 3 is not yet implemented"); }
        }

        /// <summary>
        /// Gets the electricity production of today in kWh
        /// </summary>
        public decimal ProductionToday { get; protected set; }

        /// <summary>
        /// Gets the electricity production all time in kWh
        /// </summary>
        public decimal ProductionTotal { get; protected set; }

        /// <summary>
        /// Gets the voltage on channel 1 at the solar panel side (PV Voltage DC 1)
        /// </summary>
        public decimal PVVoltageDC1 { get; protected set; }

        /// <summary>
        /// Gets the voltage on channel 2 at the solar panel side (PV Voltage DC 2)
        /// </summary>
        public decimal PVVoltageDC2 { get; protected set; }

        /// <summary>
        /// Gets the voltage on channel 3 at the solar panel side (PV Voltage DC 3)
        /// </summary>
        public decimal PVVoltageDC3 { get; protected set; }

        /// <summary>
        /// Gets the amperage on channel 1 at the solar panel side (IV Voltage DC 1)
        /// </summary>
        public decimal IVAmpsDC1 { get; protected set; }

        /// <summary>
        /// Gets the amperage on channel 2 at the solar panel side (IV Voltage DC 2)
        /// </summary>
        public decimal IVAmpsDC2 { get; protected set; }

        /// <summary>
        /// Gets the amperage on channel 3 at the solar panel side (IV Voltage DC 3)
        /// </summary>
        public decimal IVAmpsDC3 { get; protected set; }

        /// <summary>
        /// Gets the voltage on channel 1 at the electricity network side (PV Voltage AC 1)
        /// </summary>
        public decimal PVVoltageAC1 { get; protected set; }

        /// <summary>
        /// Gets the voltage on channel 2 at the electricity network side (PV Voltage AC 2)
        /// </summary>
        public decimal PVVoltageAC2 { get; protected set; }

        /// <summary>
        /// Gets the voltage on channel 3 at the electricity network side (PV Voltage AC 3)
        /// </summary>
        public decimal PVVoltageAC3 { get; protected set; }

        /// <summary>
        /// Gets the amperage on channel 1 at the electricity network side (IV Voltage AC 1)
        /// </summary>
        public decimal IVAmpsAC1 { get; protected set; }

        /// <summary>
        /// Gets the amperage on channel 2 at the electricity network side (IV Voltage AC 2)
        /// </summary>
        public decimal IVAmpsAC2 { get; protected set; }

        /// <summary>
        /// Gets the amperage on channel 3 at the electricity network side (IV Voltage AC 3)
        /// </summary>
        public decimal IVAmpsAC3 { get; protected set; }

        /// <summary>
        /// Gets the frequency of the electricity in kHz on the AC side
        /// </summary>
        public decimal FrequencyAC { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Parses the provided Omnik data into statistics
        /// </summary>
        /// <param name="omnikData">The byte array with data from the Omnik device</param>
        public Statistics(byte[] omnikData)
        {
            RawData = omnikData;

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Received {0} bytes to parse for statistics", omnikData.Length));

            ParseOmnikData(omnikData);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Parses the provided byte array with data retreived from the Omnik and puts it into this instance
        /// </summary>
        /// <param name="omnikData">Raw data as received from the Omnik</param>
        private void ParseOmnikData(byte[] omnikData)
        {
            Temperature = GetTemperatureFromRawOmnikData(omnikData);
            HoursActive = GetHoursActiveSinceLastResetFromRawOmnikData(omnikData);
            WifiModuleSerialNumber = GetWifiModuleSerialNumberFromRawOmnikData(omnikData);
            InverterSerialNumber = GetInverterSerialNumberFromRawOmnikData(omnikData);
            MainFirmwareVersion = GetMainFirmwareVersionFromRawOmnikData(omnikData);
            SlaveFirmwareVersion = GetSlaveFirmwareVersionFromRawOmnikData(omnikData);
            ProductionCurrent1 = GetCurrentElectricityProduction1FromRawOmnikData(omnikData);
            ProductionToday = GetElectricityProductionTodayFromRawOmnikData(omnikData);
            ProductionTotal = GetElectricityProductionTotalFromRawOmnikData(omnikData);
            PVVoltageDC1 = GetPVVoltageDC1FromRawOmnikData(omnikData);
            PVVoltageDC2 = GetPVVoltageDC2FromRawOmnikData(omnikData);
            PVVoltageDC3 = GetPVVoltageDC3FromRawOmnikData(omnikData);
            IVAmpsDC1 = GetIVAmpsDC1FromRawOmnikData(omnikData);
            IVAmpsDC2 = GetIVAmpsDC2FromRawOmnikData(omnikData);
            IVAmpsDC3 = GetIVAmpsDC3FromRawOmnikData(omnikData);
            PVVoltageAC1 = GetPVVoltageAC1FromRawOmnikData(omnikData);
            PVVoltageAC2 = GetPVVoltageAC2FromRawOmnikData(omnikData);
            PVVoltageAC3 = GetPVVoltageAC3FromRawOmnikData(omnikData);
            IVAmpsAC1 = GetIVAmpsAC1FromRawOmnikData(omnikData);
            IVAmpsAC2 = GetIVAmpsAC2FromRawOmnikData(omnikData);
            IVAmpsAC3 = GetIVAmpsAC3FromRawOmnikData(omnikData);
            FrequencyAC = GetFequencyACFromRawOmnikData(omnikData);
        }

        /// <summary>
        /// Gets the temperature of the Omnik device from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>Current temperature of the Omnik</returns>
        private decimal GetTemperatureFromRawOmnikData(byte[] omnikData)
        {
            var temperatureBytes = new byte[] { omnikData[32], omnikData[31], 0, 0 };
            var temperature = decimal.Divide(BitConverter.ToInt32(temperatureBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Temperature parsed from statistics: {0}", temperature));

            return temperature;
        }

        /// <summary>
        /// Gets the current electricity production of the Omnik device on channel 1 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>Current electricity production of the Omnik on channel 1 in Watts</returns>
        private int GetCurrentElectricityProduction1FromRawOmnikData(byte[] omnikData)
        {
            var currentProductionBytes = new byte[] { omnikData[60], omnikData[59], 0, 0 };
            var currentProduction = BitConverter.ToInt32(currentProductionBytes, 0);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Current electricity production on channel 1 parsed from statistics: {0} Watts", currentProduction));

            return currentProduction;
        }

        /// <summary>
        /// Gets the serial number of the wifi module from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>Serial number of the Omnik wifi module</returns>
        private string GetWifiModuleSerialNumberFromRawOmnikData(byte[] omnikData)
        {
            var serialNumberBytes = new byte[] { omnikData[4], omnikData[5], omnikData[6], omnikData[7] };
            var serialNumber = BitConverter.ToInt32(serialNumberBytes, 0);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Wifi module serial number parsed from statistics: {0}", serialNumber));

            return serialNumber.ToString();
        }

        /// <summary>
        /// Gets the serial number of the inverter from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>Serial number of the Omnik inverter</returns>
        private string GetInverterSerialNumberFromRawOmnikData(byte[] omnikData)
        {
            var inverterSerialNumber = new StringBuilder();
            for (var x = 15; x < 31; x++)
            {
                inverterSerialNumber.Append(Convert.ToChar(omnikData[x]));
            }
            var inverterSerial = inverterSerialNumber.ToString();

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Inverter serial number parsed from statistics: {0}", inverterSerial));

            return inverterSerial;
        }

        /// <summary>
        /// Gets the main firmware version of the inverter from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>Main firmware version of the Omnik inverter</returns>
        private string GetMainFirmwareVersionFromRawOmnikData(byte[] omnikData)
        {
            var inverterMainFirmwareVersionBuilder = new StringBuilder();
            for (var x = 101; x < 116; x++)
            {
                inverterMainFirmwareVersionBuilder.Append(Convert.ToChar(omnikData[x]));
            }
            var inverterMainFirmwareVersion = inverterMainFirmwareVersionBuilder.ToString();

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Main inverter firmware version parsed from statistics: {0}", inverterMainFirmwareVersion));

            return inverterMainFirmwareVersion;
        }

        /// <summary>
        /// Gets the slave firmware version of the inverter from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>Slave firmware version of the Omnik inverter</returns>
        private string GetSlaveFirmwareVersionFromRawOmnikData(byte[] omnikData)
        {
            var inverterSlaveFirmwareVersionBuilder = new StringBuilder();
            for (var x = 121; x < 130; x++)
            {
                inverterSlaveFirmwareVersionBuilder.Append(Convert.ToChar(omnikData[x]));
            }
            var inverterSlaveFirmwareVersion = inverterSlaveFirmwareVersionBuilder.ToString();

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Slave inverter firmware version parsed from statistics: {0}", inverterSlaveFirmwareVersion));

            return inverterSlaveFirmwareVersion;
        }

        /// <summary>
        /// Gets todays electricity production from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The electricity production of today in kWh</returns>
        private decimal GetElectricityProductionTodayFromRawOmnikData(byte[] omnikData)
        {
            var productionTodayBytes = new byte[] { omnikData[70], omnikData[69], 0, 0 };
            var productionToday = decimal.Divide(BitConverter.ToInt32(productionTodayBytes, 0), 100);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Electricity production today parsed from statistics: {0} kWh", productionToday));

            return productionToday;
        }

        /// <summary>
        /// Gets the electricity production since using it from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The electricity production in total since using it in kWh</returns>
        private decimal GetElectricityProductionTotalFromRawOmnikData(byte[] omnikData)
        {
            var productionTotalBytes = new byte[] { omnikData[74], omnikData[73], omnikData[72], omnikData[71] };
            var productionTotal = decimal.Divide(BitConverter.ToInt32(productionTotalBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Electricity production total parsed from statistics: {0} kWh", productionTotal));

            return productionTotal;
        }

        /// <summary>
        /// Gets the amount of hours the Omnik has been active since its last reset from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The amount of hours the Omnik has been active for since its last reset</returns>
        private int GetHoursActiveSinceLastResetFromRawOmnikData(byte[] omnikData)
        {
            var hoursActiveBytes = new byte[] { omnikData[78], omnikData[77], omnikData[76], omnikData[75] };
            var hoursActive = BitConverter.ToInt32(hoursActiveBytes, 0);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Hours active since last reset parsed from statistics: {0} hours", hoursActive));

            return hoursActive;
        }

        /// <summary>
        /// Gets the PV Voltage DC1 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The PV Voltage DC1 in volts</returns>
        private decimal GetPVVoltageDC1FromRawOmnikData(byte[] omnikData)
        {
            var pvVoltageDcBytes = new byte[] { omnikData[34], omnikData[33], 0, 0 };
            var pvVoltageDc = decimal.Divide(BitConverter.ToInt32(pvVoltageDcBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("PV Voltage DC1 parsed from statistics: {0} volt", pvVoltageDc));

            return pvVoltageDc;
        }

        /// <summary>
        /// Gets the PV Voltage DC2 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The PV Voltage DC2 in volts</returns>
        private decimal GetPVVoltageDC2FromRawOmnikData(byte[] omnikData)
        {
            var pvVoltageDcBytes = new byte[] { omnikData[36], omnikData[35], 0, 0 };
            var pvVoltageDc = decimal.Divide(BitConverter.ToInt32(pvVoltageDcBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("PV Voltage DC2 parsed from statistics: {0} volt", pvVoltageDc));

            return pvVoltageDc;
        }

        /// <summary>
        /// Gets the PV Voltage DC3 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The PV Voltage DC3 in volts</returns>
        private decimal GetPVVoltageDC3FromRawOmnikData(byte[] omnikData)
        {
            var pvVoltageDcBytes = new byte[] { omnikData[38], omnikData[37], 0, 0 };
            var pvVoltageDc = decimal.Divide(BitConverter.ToInt32(pvVoltageDcBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("PV Voltage DC3 parsed from statistics: {0} volt", pvVoltageDc));

            return pvVoltageDc;
        }

        /// <summary>
        /// Gets the IV Amps DC 1 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The IV Amps DC 1 in volts</returns>
        private decimal GetIVAmpsDC1FromRawOmnikData(byte[] omnikData)
        {
            var ivAmpsDcBytes = new byte[] { omnikData[40], omnikData[39], 0, 0 };
            var ivAmpsDc = decimal.Divide(BitConverter.ToInt32(ivAmpsDcBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("IV Amps DC 1 parsed from statistics: {0} Amps", ivAmpsDc));

            return ivAmpsDc;
        }

        /// <summary>
        /// Gets the IV Amps DC 2 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The IV Amps DC 2 in volts</returns>
        private decimal GetIVAmpsDC2FromRawOmnikData(byte[] omnikData)
        {
            var ivAmpsDcBytes = new byte[] { omnikData[42], omnikData[41], 0, 0 };
            var ivAmpsDc = decimal.Divide(BitConverter.ToInt32(ivAmpsDcBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("IV Amps DC 2 parsed from statistics: {0} Amps", ivAmpsDc));

            return ivAmpsDc;
        }

        /// <summary>
        /// Gets the IV Amps DC 3 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The IV Amps DC 3 in volts</returns>
        private decimal GetIVAmpsDC3FromRawOmnikData(byte[] omnikData)
        {
            var ivAmpsDcBytes = new byte[] { omnikData[44], omnikData[43], 0, 0 };
            var ivAmpsDc = decimal.Divide(BitConverter.ToInt32(ivAmpsDcBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("IV Amps DC 3 parsed from statistics: {0} Amps", ivAmpsDc));

            return ivAmpsDc;
        }

        /// <summary>
        /// Gets the PV Voltage AC 1 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The PV Voltage AC 1 in volts</returns>
        private decimal GetPVVoltageAC1FromRawOmnikData(byte[] omnikData)
        {
            var pvVoltageAcBytes = new byte[] { omnikData[52], omnikData[51], 0, 0 };
            var pvVoltageAc = decimal.Divide(BitConverter.ToInt32(pvVoltageAcBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("PV Voltage AC 1 parsed from statistics: {0} volt", pvVoltageAc));

            return pvVoltageAc;
        }

        /// <summary>
        /// Gets the PV Voltage AC 2 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The PV Voltage AC 2 in volts</returns>
        private decimal GetPVVoltageAC2FromRawOmnikData(byte[] omnikData)
        {
            var pvVoltageAcBytes = new byte[] { omnikData[54], omnikData[53], 0, 0 };
            var pvVoltageAc = decimal.Divide(BitConverter.ToInt32(pvVoltageAcBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("PV Voltage AC 2 parsed from statistics: {0} volt", pvVoltageAc));

            return pvVoltageAc;
        }

        /// <summary>
        /// Gets the PV Voltage AC 3 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The PV Voltage AC 3 in volts</returns>
        private decimal GetPVVoltageAC3FromRawOmnikData(byte[] omnikData)
        {
            var pvVoltageAcBytes = new byte[] { omnikData[56], omnikData[55], 0, 0 };
            var pvVoltageAc = decimal.Divide(BitConverter.ToInt32(pvVoltageAcBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("PV Voltage AC 3 parsed from statistics: {0} volt", pvVoltageAc));

            return pvVoltageAc;
        }

        /// <summary>
        /// Gets the IV Amps AC 1 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The IV Amps AC 1 in volts</returns>
        private decimal GetIVAmpsAC1FromRawOmnikData(byte[] omnikData)
        {
            var ivAmpsAcBytes = new byte[] { omnikData[46], omnikData[45], 0, 0 };
            var ivAmpsAc = decimal.Divide(BitConverter.ToInt32(ivAmpsAcBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("IV Voltage AC 1 parsed from statistics: {0} Amps", ivAmpsAc));

            return ivAmpsAc;
        }

        /// <summary>
        /// Gets the IV Amps AC 2 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The IV Amps AC 2 in volts</returns>
        private decimal GetIVAmpsAC2FromRawOmnikData(byte[] omnikData)
        {
            var ivAmpsAcBytes = new byte[] { omnikData[48], omnikData[47], 0, 0 };
            var ivAmpsAc = decimal.Divide(BitConverter.ToInt32(ivAmpsAcBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("IV Voltage AC 2 parsed from statistics: {0} Amps", ivAmpsAc));

            return ivAmpsAc;
        }

        /// <summary>
        /// Gets the IV Amps AC 3 from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The IV Amps AC 3 in volts</returns>
        private decimal GetIVAmpsAC3FromRawOmnikData(byte[] omnikData)
        {
            var ivAmpsAcBytes = new byte[] { omnikData[50], omnikData[49], 0, 0 };
            var ivAmpsAc = decimal.Divide(BitConverter.ToInt32(ivAmpsAcBytes, 0), 10);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("IV Voltage AC 3 parsed from statistics: {0} Amps", ivAmpsAc));

            return ivAmpsAc;
        }

        /// <summary>
        /// Gets the Frequency AC from the raw Omnik data
        /// </summary>
        /// <param name="omnikData">Raw Omnik data</param>
        /// <returns>The frequency of the electiricy at the AC side in Hz</returns>
        private decimal GetFequencyACFromRawOmnikData(byte[] omnikData)
        {
            var frequencyAcBytes = new byte[] { omnikData[58], omnikData[57], 0, 0 };
            var frequencyAc = decimal.Divide(BitConverter.ToInt32(frequencyAcBytes, 0), 100);

            Logging.Logdata.LogMessage(TraceLevel.Verbose, string.Format("Frequency AC side parsed from statistics: {0} Hz", frequencyAc));

            return frequencyAc;
        }

        #endregion
    }
}
