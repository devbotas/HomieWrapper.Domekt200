using System;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;
using NLog;

namespace HomieWrapper {
    partial class Domekt200 {
        private HostDevice _device;

        private IMqttBroker _broker = new PahoBroker();
        private ReliableModbus _reliableModbus = new ReliableModbus();

        HostTextProperty _actualDateTimeProperty;
        HostChoiceProperty _actualState;
        HostChoiceProperty _targetState;
        HostChoiceProperty _actualVentilationLevelProperty;
        HostNumberProperty _supplyAirTemperatureProperty;
        HostChoiceProperty _actualModbusConnectionState;
        HostNumberProperty _disconnectCount;

        private DateTime _startTime = DateTime.Now;
        private HostNumberProperty _systemUptime;

        public static Logger Log = LogManager.GetLogger("HomieWrapper.Domekt200");

        public Domekt200() { }
    }
}
