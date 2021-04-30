﻿namespace SharpModbus {
    public class ModbusF15WriteCoils : IModbusCommand {
        private readonly bool[] _values;

        public byte Code { get { return 15; } }
        public byte Slave { get; }
        public ushort Address { get; }
        public bool[] Values { get { return ModbusHelper.Clone(_values); } }
        public int RequestLength { get { return 7 + ModbusHelper.BytesForBools(_values.Length); } }
        public int ResponseLength { get { return 6; } }

        public ModbusF15WriteCoils(byte slave, ushort address, bool[] values) {
            Slave = slave;
            Address = address;
            _values = values;
        }

        public void FillRequest(byte[] request, int offset) {
            FillResponse(request, offset, null);
            var bytes = ModbusHelper.EncodeBools(_values);
            request[offset + 6] = (byte)bytes.Length;
            ModbusHelper.Copy(bytes, 0, request, offset + 7, bytes.Length);
        }

        public object ParseResponse(byte[] response, int offset) {
            Tools.AssertEqual(response[offset + 0], Slave, "Slave mismatch got {0} expected {1}");
            Tools.AssertEqual(response[offset + 1], 15, "Function mismatch got {0} expected {1}");
            Tools.AssertEqual(ModbusHelper.GetUShort(response, offset + 2), Address, "Address mismatch got {0} expected {1}");
            Tools.AssertEqual(ModbusHelper.GetUShort(response, offset + 4), _values.Length, "Coil count mismatch got {0} expected {1}");
            return null;
        }

        public void FillResponse(byte[] response, int offset, object value) {
            response[offset + 0] = Slave;
            response[offset + 1] = 15;
            response[offset + 2] = ModbusHelper.High(Address);
            response[offset + 3] = ModbusHelper.Low(Address);
            response[offset + 4] = ModbusHelper.High(_values.Length);
            response[offset + 5] = ModbusHelper.Low(_values.Length);
        }

        public override string ToString() {
            return string.Format("[ModbusF15WriteCoils Slave={0}, Address={1}, Values={2}]", Slave, Address, _values);
        }
    }
}
