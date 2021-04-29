using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;


namespace HomieWrapper {
    class ReliableModbus {
        public bool IsConnected { get; private set; }
        public bool IsInitialized { get; private set; }
        public Exception LastException { get; private set; }

        public void Initialize(string modbusDeviceIpAddress) {
            if (IsInitialized) { return; }

            _globalCancellationTokenSource = new CancellationTokenSource();

            _deviceIp = modbusDeviceIpAddress;

            _modbus = new SharpModbus.ModbusMaster(ReceiveFromSocket, SendToSocket);

            Task.Run(async () => await MonitorConnectionContinuously(_globalCancellationTokenSource.Token));

            IsInitialized = true;
        }
        public async Task MonitorConnectionContinuously(CancellationToken cancelationToken) {
            while (cancelationToken.IsCancellationRequested == false) {
                if (IsConnected == false) {
                    try {
                        _log.Info($"Connecting to Modbus device at {_deviceIp}.");
                        Connect();

                        _modbus.ReadFromDevice = ReceiveFromSocket;

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

                value = _modbus.ReadHoldingRegister(2, (ushort)((ushort)register - 1));
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

                _modbus.WriteRegister(2, (ushort)((ushort)register - 1), (ushort)value);

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
        private IExceptionlessSocket _socket;

        private SharpModbus.ModbusMaster _modbus;

        private void Connect() {
            try {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveTimeout = 5000;
                socket.SendTimeout = 1000;

                if (_deviceIp.ToLower() == "localhost") {
                    // Localhost works fine on Windows, but we experienced problems on our Torizon Linux machines.
                    // Also, connecting to "localhost" will have to go to DNS which is much slower than simply using 127.0.0.1.
                    // Therefore I replace it here and hopefully save some debugging hours.
                    socket.Connect("127.0.0.1", 502);
                }
                else {
                    socket.Connect(_deviceIp, 502);
                }

                _socket = new TcpSocketWrapper(socket);

                IsConnected = true;
            }
            catch (SocketException ex) {
                LastException = ex;
                DisconnectAndCleanup();
            }
        }
        private void DisconnectAndCleanup() {
            _socket?.Dispose();
            _socket = null;
            IsConnected = false;
        }

        private int ReceiveFromSocket(byte[] buffer) {
            var returnByteCount = 0;
            var tempBuffer = new byte[128];

            lock (_modbusLock) {
                if (IsConnected) {
                    var receiveResult = _socket.TryReceive(tempBuffer, 0, 9, 128);
                    if (receiveResult.IsOk) {
                        returnByteCount = receiveResult.Count;
                        Array.Copy(tempBuffer, 0, buffer, 0, returnByteCount);
                    }
                    else {
                        DisconnectAndCleanup();
                    }
                }
            }

            return returnByteCount;
        }

        private void SendToSocket(byte[] buffer) {
            lock (_modbusLock) {
                if (IsConnected) {
                    var isOk = _socket.TrySend(buffer, 0, buffer.Length);
                    if (isOk == false) {
                        DisconnectAndCleanup();
                    }
                }
            }
        }
    }
}
