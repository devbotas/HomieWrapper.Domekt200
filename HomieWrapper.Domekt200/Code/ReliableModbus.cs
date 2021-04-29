using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AMWD.Modbus.Tcp.Client;
using NLog;


namespace HomieWrapper {
    class ReliableModbus {
        public bool IsConnected { get; private set; }
        public bool IsInitialized { get; private set; }

        public void Initialize(string modbusDeviceIpAddress) {
            if (IsInitialized) { return; }

            _globalCancellationTokenSource = new CancellationTokenSource();

            _deviceIp = modbusDeviceIpAddress;

            _modbus = new ModbusClient(_deviceIp, 502);
            _modbus.MaxConnectTimeout = TimeSpan.FromSeconds(2);
            _modbus.ReconnectTimeSpan = TimeSpan.FromSeconds(2);
            _modbus.Disconnected += (sender, e) => {
                IsConnected = false;
                _log.Error("AMWD library reports ModBus disconnection.");
            };

            Task.Run(async () => await MonitorConnectionContinuously(_globalCancellationTokenSource.Token));

            IsInitialized = true;
        }
        public async Task MonitorConnectionContinuously(CancellationToken cancelationToken) {
            while (cancelationToken.IsCancellationRequested == false) {
                if (IsConnected == false) {
                    try {
                        _log.Info($"Connecting to Modbus device at {_deviceIp}.");
                        _modbus.Connect().Wait();

                        IsConnected = true;
                    }
                    catch (Exception ex) {
                        _log.Error(ex, $"{nameof(MonitorConnectionContinuously)} tried to connect to broker, but that did not work.");
                    }
                }
                await Task.Delay(1000, cancelationToken);
            }
        }

        int sendCounter = 0;
        public bool TryReadModbusRegister(KomfoventRegisters register, out int value) {
            if (IsInitialized == false) {
                value = 0;
                return false;
            }

            sendCounter++;

            var returnResult = false;
            value = 0;

            try {
                //lock (_modbusLock) {
                //_modbus.Connect().Wait();
                ushort taskReturnValue = 0;
                var bybis = new CancellationTokenSource();
                bybis.CancelAfter(1000);

                if (_modbus.IsConnected == false) {
                    var papai = 1;
                }

                //  Debug.WriteLine(_modbus.ConnectingTask);


                var pyzdaTask = _modbus.ReadHoldingRegisters(2, (ushort)((ushort)register - 1), 1, bybis.Token);
                var bybiWatch = Stopwatch.StartNew();
                var registers = pyzdaTask.Result;
                bybiWatch.Stop();
                Debug.WriteLine($"Bybiwatch: {bybiWatch.ElapsedMilliseconds}");

                if (pyzdaTask.Status == TaskStatus.RanToCompletion) {
                    if (registers != null) {
                        if (registers.Count == 0) { var pzdc = 1; }

                        taskReturnValue = registers[0].RegisterValue;
                    }
                }
                else {
                    var pzdc = 1;
                }

                //_modbus.Disconnect().Wait();
                Thread.Sleep(1000);

                value = taskReturnValue;
                //}
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
                    _modbus.WriteSingleRegister(2, new AMWD.Modbus.Common.Structures.ModbusObject() { Address = (ushort)((ushort)register - 1), RegisterValue = (ushort)value, Type = AMWD.Modbus.Common.ModbusObjectType.HoldingRegister });
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
        ModbusClient _modbus;

        private object _modbusLock = new object();
    }
}
