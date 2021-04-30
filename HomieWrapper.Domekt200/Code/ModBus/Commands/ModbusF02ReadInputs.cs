using static SharpModbus.ModbusHelper;

namespace SharpModbus {
    public class ModbusF02ReadInputs : IModbusCommand {
        public byte Code { get { return 2; } }
        public byte Slave { get; }
        public ushort Address { get; }
        public ushort Count { get; }
        public int RequestLength { get { return 6; } }
        public int ResponseLength { get { return 3 + BytesForBools(Count); } }

        public ModbusF02ReadInputs(byte slave, ushort address, ushort count) {
            Slave = slave;
            Address = address;
            Count = count;
        }

        public void FillRequest(byte[] request, int offset) {
            request[offset + 0] = Slave;
            request[offset + 1] = 2;
            request[offset + 2] = High(Address);
            request[offset + 3] = Low(Address);
            request[offset + 4] = High(Count);
            request[offset + 5] = Low(Count);
        }

        public object ParseResponse(byte[] response, int offset) {
            var bytes = BytesForBools(Count);
            AssertEqual(response[offset + 0], Slave, "Slave mismatch got {0} expected {1}");
            AssertEqual(response[offset + 1], 2, "Function mismatch got {0} expected {1}");
            AssertEqual(response[offset + 2], bytes, "Bytes mismatch got {0} expected {1}");
            return DecodeBools(response, offset + 3, Count);
        }

        public void FillResponse(byte[] response, int offset, object value) {
            var bytes = BytesForBools(Count);
            response[offset + 0] = Slave;
            response[offset + 1] = 2;
            response[offset + 2] = bytes;
            var data = EncodeBools(value as bool[]);
            Copy(data, 0, response, offset + 3, bytes);
        }

        public override string ToString() {
            return string.Format("[ModbusF02ReadInputs Slave={0}, Address={1}, Count={2}]", Slave, Address, Count);
        }
    }
}
