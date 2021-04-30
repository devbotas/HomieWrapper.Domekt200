﻿namespace SharpModbus {
    public class ModbusF01ReadCoils : IModbusCommand {
        public byte Code { get { return 1; } }
        public byte Slave { get; }
        public ushort Address { get; }
        public ushort Count { get; }
        public int RequestLength { get { return 6; } }
        public int ResponseLength { get { return 3 + ModbusHelper.BytesForBools(Count); } }

        public ModbusF01ReadCoils(byte slave, ushort address, ushort count) {
            Slave = slave;
            Address = address;
            Count = count;
        }

        public void FillRequest(byte[] request, int offset) {
            request[offset + 0] = Slave;
            request[offset + 1] = 1;
            request[offset + 2] = ModbusHelper.High(Address);
            request[offset + 3] = ModbusHelper.Low(Address);
            request[offset + 4] = ModbusHelper.High(Count);
            request[offset + 5] = ModbusHelper.Low(Count);
        }

        public object ParseResponse(byte[] response, int offset) {
            var bytes = ModbusHelper.BytesForBools(Count);
            Tools.AssertEqual(response[offset + 0], Slave, "Slave mismatch got {0} expected {1}");
            Tools.AssertEqual(response[offset + 1], 1, "Function mismatch got {0} expected {1}");
            Tools.AssertEqual(response[offset + 2], bytes, "Bytes mismatch got {0} expected {1}");
            return ModbusHelper.DecodeBools(response, offset + 3, Count);
        }

        public object ApplyTo(ModbusModel model) {
            return model.GetDOs(Slave, Address, Count);
        }

        public void FillResponse(byte[] response, int offset, object value) {
            var bytes = ModbusHelper.BytesForBools(Count);
            response[offset + 0] = Slave;
            response[offset + 1] = 1;
            response[offset + 2] = bytes;
            var data = ModbusHelper.EncodeBools(value as bool[]);
            ModbusHelper.Copy(data, 0, response, offset + 3, bytes);
        }

        public override string ToString() {
            return string.Format("[ModbusF01ReadCoils Slave={0}, Address={1}, Count={2}]", Slave, Address, Count);
        }
    }
}
