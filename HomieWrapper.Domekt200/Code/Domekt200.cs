using System;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;
using NLog;

namespace HomieWrapper {
    partial class Domekt200 {
        private HostDevice _device;

        private ResilientHomieBroker _broker = new ResilientHomieBroker();
        private ReliableModbus _reliableModbus = new ReliableModbus();

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
