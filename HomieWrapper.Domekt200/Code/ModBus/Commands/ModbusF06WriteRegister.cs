namespace SharpModbus {
    public class ModbusF06WriteRegister : IModbusCommand {
        public byte Code { get { return 6; } }
        public byte Slave { get; }
        public ushort Address { get; }
        public ushort Value { get; }
        public int RequestLength { get { return 6; } }
        public int ResponseLength { get { return 6; } }

        public ModbusF06WriteRegister(byte slave, ushort address, ushort value) {
            Slave = slave;
            Address = address;
            Value = value;
        }

        public void FillRequest(byte[] request, int offset) {
            request[offset + 0] = Slave;
            request[offset + 1] = 6;
            request[offset + 2] = ModbusHelper.High(Address);
            request[offset + 3] = ModbusHelper.Low(Address);
            request[offset + 4] = ModbusHelper.High(Value);
            request[offset + 5] = ModbusHelper.Low(Value);
        }

        public object ParseResponse(byte[] response, int offset) {
            Tools.AssertEqual(response[offset + 0], Slave, "Slave mismatch got {0} expected {1}");
            Tools.AssertEqual(response[offset + 1], 6, "Function mismatch got {0} expected {1}");
            Tools.AssertEqual(ModbusHelper.GetUShort(response, offset + 2), Address, "Address mismatch got {0} expected {1}");
            Tools.AssertEqual(ModbusHelper.GetUShort(response, offset + 4), Value, "Value mismatch got {0} expected {1}");
            return null;
        }

        public object ApplyTo(ModbusModel model) {
            model.SetWO(Slave, Address, Value);
            return null;
        }

        public void FillResponse(byte[] response, int offset, object value) {
            FillRequest(response, offset);
        }

        public override string ToString() {
            return string.Format("[ModbusF06WriteRegister Slave={0}, Address={1}, Value={2}]", Slave, Address, Value);
        }
    }
}
