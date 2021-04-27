using System;
using System.IO;
using System.Text;
using System.Xml;
using DevBot9.Protocols.Homie;
using NLog;
using NLog.Config;

namespace HomieWrapper.Domekt200 {
    class Program {
        private static Domekt200 _recuperator = new Domekt200();

        public static Logger Log = LogManager.GetLogger("ShedMonitor.Main");
        static void Main(string[] args) {
            // Load environment variables.
            var brokerIp = Environment.GetEnvironmentVariable("MQTT_BROKER_IP");
            if (string.IsNullOrEmpty(brokerIp)) {
                Console.WriteLine("Evironment variable \"MQTT_BROKER_IP\" is not provided. Using 127.0.0.1.");
                brokerIp = "127.0.0.1";
            }
            var domektIp = Environment.GetEnvironmentVariable("DOMEKT_IP");
            if (string.IsNullOrEmpty(domektIp)) {
                Console.WriteLine("Evironment variable \"DOMEKT_IP\" is not provided. Using 127.0.0.1.");
                domektIp = "127.0.0.1";
            }

            var logsFolder = Directory.GetCurrentDirectory(); // Path.Combine(LogsFolder);

            // NLog doesn't like backslashes.
            logsFolder = logsFolder.Replace("\\", "/");

            // Finalizing configuration.
            LogManager.Configuration = new XmlLoggingConfiguration(new XmlTextReader(new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.NLogConfig.Replace("!LogsFolderTag!", logsFolder)))), "NLogConfig.xml");

            Log.Info("Application started.");
            DeviceFactory.Initialize("homie");
            _recuperator.Initialize(brokerIp, domektIp);

            Console.ReadLine();
        }
    }
}
