using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;


namespace HomieWrapper {
    class ReliableModbus {
        public bool IsConnected { get; private set; }
        public bool IsInitialized { get; private set; }

        public void Initialize(string modbusDeviceIpAddress) {
            if (IsInitialized) { return; }

            _globalCancellationTokenSource = new CancellationTokenSource();

            _deviceIp = modbusDeviceIpAddress;

            Task.Run(async () => await MonitorConnectionContinuously(_globalCancellationTokenSource.Token));

            IsInitialized = true;
        }
        public async Task MonitorConnectionContinuously(CancellationToken cancelationToken) {
            while (cancelationToken.IsCancellationRequested == false) {
                if (IsConnected == false) {
                    try {
                        _log.Info($"Connecting to Modbus device at {_deviceIp}.");

                        IsConnected = true;
                    }
                    catch (Exception ex) {
                        _log.Error(ex, $"{nameof(MonitorConnectionContinuously)} tried to connect to broker, but that did not work.");
                    }
                }
                await Task.Delay(1000, cancelationToken);
            }
        }

        public bool TryReadModbusRegister(KomfoventRegisters register, out int value) {
            if (IsInitialized == false) {
                value = 0;
                return false;
            }


            var returnResult = false;
            value = 0;

            try {
                //var pyzdaTask = _modbus.ReadHoldingRegisters(2, (ushort)((ushort)register - 1), 1);
                //var bybiWatch = Stopwatch.StartNew();
                //var registers = pyzdaTask.Result;
                //bybiWatch.Stop();
                //Debug.WriteLine($"Bybiwatch: {bybiWatch.ElapsedMilliseconds}");

                //if (pyzdaTask.Status == TaskStatus.RanToCompletion) {
                //    if (registers != null) {
                //        if (registers.Count == 0) { var pzdc = 1; }

                //        value = registers[0].RegisterValue;
                //    }
                //}
                //else {
                //    var pzdc = 1;
                //}

                Thread.Sleep(10);


                returnResult = true;
            }
            catch (Exception ex) {
                _log.Warn($"Could not read ModBus register {register}, because of {ex.Message}.");
                IsConnected = false;
            }

            return returnResult;
        }

        public bool TryWriteModbusRegister(KomfoventRegisters register, int value) {
            if (IsInitialized == false) { return false; }

            var returnResult = false;

            try {
                lock (_modbusLock) {
                    // _modbus.WriteSingleRegister(2, new AMWD.Modbus.Common.Structures.ModbusObject() { Address = (ushort)((ushort)register - 1), RegisterValue = (ushort)value, Type = AMWD.Modbus.Common.ModbusObjectType.HoldingRegister });
                }
            }
            catch (Exception ex) {
                _log.Warn($"Could not write ModBus register {register}, because of {ex.Message}.");
            }

            return returnResult;
        }

        private CancellationTokenSource _globalCancellationTokenSource;
        private Logger _log = LogManager.GetCurrentClassLogger();
        private string _deviceIp = "localhost";

        private object _modbusLock = new object();
    }
}
