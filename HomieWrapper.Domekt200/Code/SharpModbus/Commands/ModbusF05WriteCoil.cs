using static SharpModbus.ModbusHelper;

namespace SharpModbus {
    public class ModbusF05WriteCoil : IModbusCommand {
        public byte Code { get { return 5; } }
        public byte Slave { get; }
        public ushort Address { get; }
        public bool Value { get; }
        public int RequestLength { get { return 6; } }
        public int ResponseLength { get { return 6; } }

        public ModbusF05WriteCoil(byte slave, ushort address, bool state) {
            Slave = slave;
            Address = address;
            Value = state;
        }

        public void FillRequest(byte[] request, int offset) {
            request[offset + 0] = Slave;
            request[offset + 1] = 5;
            request[offset + 2] = High(Address);
            request[offset + 3] = Low(Address);
            request[offset + 4] = EncodeBool(Value);
            request[offset + 5] = 0;
        }

        public object ParseResponse(byte[] response, int offset) {
            AssertEqual(response[offset + 0], Slave, "Slave mismatch got {0} expected {1}");
            AssertEqual(response[offset + 1], 5, "Function mismatch got {0} expected {1}");
            AssertEqual(GetUShort(response, offset + 2), Address, "Address mismatch got {0} expected {1}");
            AssertEqual(response[offset + 4], EncodeBool(Value), "Value mismatch got {0} expected {1}");
            AssertEqual(response[offset + 5], 0, "Pad mismatch {0} expected:{1}");
            return null;
        }

        public void FillResponse(byte[] response, int offset, object value) {
            FillRequest(response, offset);
        }

        public override string ToString() {
            return string.Format("[ModbusF05WriteCoil Slave={0}, Address={1}, Value={2}]", Slave, Address, Value);
        }
    }
}
