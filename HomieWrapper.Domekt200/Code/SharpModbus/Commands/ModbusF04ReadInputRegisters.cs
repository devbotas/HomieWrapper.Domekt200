using static SharpModbus.ModbusHelper;

namespace SharpModbus {
    public class ModbusF04ReadInputRegisters : IModbusCommand {
        public byte Code { get { return 4; } }
        public byte Slave { get; }
        public ushort Address { get; }
        public ushort Count { get; }
        public int RequestLength { get { return 6; } }
        public int ResponseLength { get { return 3 + BytesForWords(Count); } }

        public ModbusF04ReadInputRegisters(byte slave, ushort address, ushort count) {
            Slave = slave;
            Address = address;
            Count = count;
        }

        public void FillRequest(byte[] request, int offset) {
            request[offset + 0] = Slave;
            request[offset + 1] = 4;
            request[offset + 2] = High(Address);
            request[offset + 3] = Low(Address);
            request[offset + 4] = High(Count);
            request[offset + 5] = Low(Count);
        }

        public object ParseResponse(byte[] response, int offset) {
            var bytes = ModbusHelper.BytesForWords(Count);
            AssertEqual(response[offset + 0], Slave, "Slave mismatch got {0} expected {1}");
            AssertEqual(response[offset + 1], 4, "Function mismatch got {0} expected {1}");
            AssertEqual(response[offset + 2], bytes, "Bytes mismatch got {0} expected {1}");
            return DecodeWords(response, offset + 3, Count);
        }

        public void FillResponse(byte[] response, int offset, object value) {
            var bytes = BytesForWords(Count);
            response[offset + 0] = Slave;
            response[offset + 1] = 4;
            response[offset + 2] = bytes;
            var data = EncodeWords(value as ushort[]);
            Copy(data, 0, response, offset + 3, bytes);
        }

        public override string ToString() {
            return string.Format("[ModbusF04ReadInputRegisters Slave={0}, Address={1}, Count={2}]", Slave, Address, Count);
        }
    }
}
