using System;
using AMWD.Modbus.Tcp.Client;
using DevBot9.Protocols.Homie;
using NLog;

namespace HomieWrapper {
    partial class Domekt200 {
        private HostDevice _device;

        private ReliableBroker _reliableBroker;

        ModbusClient _modbus;
        private object _modbusLock = new object();
        HostStringProperty _actualDateTimeProperty;
        HostEnumProperty _actualState;
        HostEnumProperty _targetState;
        HostEnumProperty _actualVentilationLevelProperty;
        HostFloatProperty _supplyAirTemperatureProperty;

        private DateTime _startTime = DateTime.Now;
        private HostFloatProperty _systemUptime;

        public static Logger Log = LogManager.GetLogger("HomieWrapper.Domekt200");

        public Domekt200() {

        }
    }
}
