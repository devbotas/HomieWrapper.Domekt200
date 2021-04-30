using System.Threading;
using NLog;

namespace SharpModbus {
    public class ModbusMaster {
        private Logger _log = LogManager.GetCurrentClassLogger();

        public delegate bool WriteReadDeviceDelegate(byte[] sendBuffer, byte[] receiveBuffer);

        public WriteReadDeviceDelegate WriteReadDevice;

        public ushort TransactionId { get; set; } = 0;

        public ModbusMaster() {
        }

        public void Initialize(WriteReadDeviceDelegate writeReadDelegate) {
            WriteReadDevice = writeReadDelegate;
        }

        public bool ReadCoil(byte slave, ushort address) {
            return ReadCoils(slave, address, 1)[0]; //there is no code for single read
        }

        public bool ReadInput(byte slave, ushort address) {
            return ReadInputs(slave, address, 1)[0]; //there is no code for single read
        }

        public ushort ReadInputRegister(byte slave, ushort address) {
            return ReadInputRegisters(slave, address, 1)[0]; //there is no code for single read
        }

        public ushort ReadHoldingRegister(byte slave, ushort address) {
            return ReadHoldingRegisters(slave, address, 1)[0]; //there is no code for single read
        }

        public bool[] ReadCoils(byte slave, ushort address, ushort count) {
            return Execute(new ModbusF01ReadCoils(slave, address, count)) as bool[];
        }

        public bool[] ReadInputs(byte slave, ushort address, ushort count) {
            return Execute(new ModbusF02ReadInputs(slave, address, count)) as bool[];
        }

        public ushort[] ReadInputRegisters(byte slave, ushort address, ushort count) {
            return Execute(new ModbusF04ReadInputRegisters(slave, address, count)) as ushort[];
        }

        public ushort[] ReadHoldingRegisters(byte slave, ushort address, ushort count) {
            return Execute(new ModbusF03ReadHoldingRegisters(slave, address, count)) as ushort[];
        }

        public void WriteCoil(byte slave, ushort address, bool value) {
            Execute(new ModbusF05WriteCoil(slave, address, value));
        }

        public void WriteRegister(byte slave, ushort address, ushort value) {
            Execute(new ModbusF06WriteRegister(slave, address, value));
        }

        public void WriteCoils(byte slave, ushort address, params bool[] values) {
            Execute(new ModbusF15WriteCoils(slave, address, values));
        }

        public void WriteRegisters(byte slave, ushort address, params ushort[] values) {
            Execute(new ModbusF16WriteRegisters(slave, address, values));
        }

        private object Execute(IModbusCommand cmd) {
            var wrapper = new ModbusTCPWrapper(cmd, TransactionId++);
            var request = new byte[wrapper.RequestLength];
            var response = new byte[wrapper.ResponseLength];
            wrapper.FillRequest(request, 0);

            var isOk = WriteReadDevice(request, response);

            if (isOk == false) {
                _log.Warn("Resending request.");
                Thread.Sleep(100);
                WriteReadDevice(request, response);
            }

            return wrapper.ParseResponse(response, 0);
        }
    }
}
