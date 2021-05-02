using System;
using DevBot9.Protocols.Homie;
using NLog;

namespace HomieWrapper {
    partial class Domekt200 {
        private HostDevice _device;

        private ReliableBroker _reliableBroker;
        private ReliableModbus _reliableModbus;

        HostStringProperty _actualDateTimeProperty;
        HostEnumProperty _actualState;
        HostEnumProperty _targetState;
        HostEnumProperty _actualVentilationLevelProperty;
        HostFloatProperty _supplyAirTemperatureProperty;
        HostEnumProperty _actualModbusConnectionState;
        HostIntegerProperty _disconnectCount;

        private DateTime _startTime = DateTime.Now;
        private HostFloatProperty _systemUptime;

        public static Logger Log = LogManager.GetLogger("HomieWrapper.Domekt200");

        public Domekt200() { }
    }
}
