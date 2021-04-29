using System;
using System.IO;
using System.Text;
using System.Xml;
using DevBot9.Protocols.Homie;
using NLog;
using NLog.Config;

namespace HomieWrapper {
    class Program {
        private static Domekt200 _recuperator = new Domekt200();
        private static ReliableBroker _reliableBroker = new ReliableBroker();
        private static ReliableModbus _reliableModbus = new ReliableModbus();

        public static Logger Log = LogManager.GetLogger("HomieWrapper.Main");
        static void Main(string[] args) {
            var logsFolder = Directory.GetCurrentDirectory();
            // NLog doesn't like backslashes.
            logsFolder = logsFolder.Replace("\\", "/");
            // Finalizing configuration.
            LogManager.Configuration = new XmlLoggingConfiguration(new XmlTextReader(new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.NLogConfig.Replace("!LogsFolderTag!", logsFolder)))), "NLogConfig.xml");


            // Load environment variables.
            var brokerIp = Environment.GetEnvironmentVariable("MQTT_BROKER_IP");
            if (string.IsNullOrEmpty(brokerIp)) {
                Log.Warn("Evironment variable \"MQTT_BROKER_IP\" is not provided. Using 127.0.0.1.");
                brokerIp = "127.0.0.1";
            }
            var domektIp = Environment.GetEnvironmentVariable("DOMEKT_IP");
            if (string.IsNullOrEmpty(domektIp)) {
                Log.Warn("Evironment variable \"DOMEKT_IP\" is not provided. Using 127.0.0.1.");
                domektIp = "127.0.0.1";
            }

            Log.Info("Application started.");
            DeviceFactory.Initialize("homie");
            _reliableBroker.Initialize(brokerIp);
            _reliableModbus.Initialize(domektIp);
            _recuperator.Initialize(_reliableBroker, _reliableModbus);

            Console.ReadLine();
        }
    }
}
