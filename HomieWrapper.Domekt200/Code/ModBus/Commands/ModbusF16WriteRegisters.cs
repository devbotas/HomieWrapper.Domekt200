using static SharpModbus.ModbusHelper;

namespace SharpModbus {
    public class ModbusF16WriteRegisters : IModbusCommand {
        private readonly ushort[] _values;

        public byte Code { get { return 16; } }
        public byte Slave { get; }
        public ushort Address { get; }
        public ushort[] Values { get { return Clone(_values); } }
        public int RequestLength { get { return 7 + BytesForWords(_values.Length); } }
        public int ResponseLength { get { return 6; } }

        public ModbusF16WriteRegisters(byte slave, ushort address, ushort[] values) {
            Slave = slave;
            Address = address;
            _values = values;
        }

        public void FillRequest(byte[] request, int offset) {
            FillResponse(request, offset, null);
            var bytes = EncodeWords(_values);
            request[offset + 6] = (byte)bytes.Length;
            Copy(bytes, 0, request, offset + 7, bytes.Length);
        }

        public object ParseResponse(byte[] response, int offset) {
            AssertEqual(response[offset + 0], Slave, "Slave mismatch got {0} expected {1}");
            AssertEqual(response[offset + 1], 16, "Function mismatch got {0} expected {1}");
            AssertEqual(GetUShort(response, offset + 2), Address, "Address mismatch got {0} expected {1}");
            AssertEqual(GetUShort(response, offset + 4), _values.Length, "Register count mismatch got {0} expected {1}");
            return null;
        }

        public void FillResponse(byte[] response, int offset, object value) {
            response[offset + 0] = Slave;
            response[offset + 1] = 16;
            response[offset + 2] = High(Address);
            response[offset + 3] = Low(Address);
            response[offset + 4] = High(_values.Length);
            response[offset + 5] = Low(_values.Length);
        }

        public override string ToString() {
            return string.Format("[ModbusF16WriteRegisters Slave={0}, Address={1}, Values={2}]", Slave, Address, _values);
        }
    }
}
