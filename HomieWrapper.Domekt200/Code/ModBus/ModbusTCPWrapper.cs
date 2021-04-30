namespace SharpModbus {
    public class ModbusTCPWrapper {
        public byte Code { get { return Wrapped.Code; } }
        public byte Slave { get { return Wrapped.Slave; } }
        public ushort Address { get { return Wrapped.Address; } }
        public IModbusCommand Wrapped { get; }
        public ushort TransactionId { get; }
        public int RequestLength { get { return Wrapped.RequestLength + 6; } }
        public int ResponseLength { get { return Wrapped.ResponseLength + 6; } }

        public ModbusTCPWrapper(IModbusCommand wrapped, ushort transactionId) {
            Wrapped = wrapped;
            TransactionId = transactionId;
        }

        public void FillRequest(byte[] request, int offset) {
            request[offset + 0] = ModbusHelper.High(TransactionId);
            request[offset + 1] = ModbusHelper.Low(TransactionId);
            request[offset + 2] = 0;
            request[offset + 3] = 0;
            request[offset + 4] = ModbusHelper.High(Wrapped.RequestLength);
            request[offset + 5] = ModbusHelper.Low(Wrapped.RequestLength);
            Wrapped.FillRequest(request, offset + 6);
        }

        public object ParseResponse(byte[] response, int offset) {
            Tools.AssertEqual(ModbusHelper.GetUShort(response, offset + 0), TransactionId, "TransactionId mismatch got {0} expected {1}");
            Tools.AssertEqual(ModbusHelper.GetUShort(response, offset + 2), 0, "Zero mismatch got {0} expected {1}");
            Tools.AssertEqual(ModbusHelper.GetUShort(response, offset + 4), Wrapped.ResponseLength, "Length mismatch got {0} expected {1}");
            return Wrapped.ParseResponse(response, offset + 6);
        }

        public void FillResponse(byte[] response, int offset, object value) {
            response[offset + 0] = ModbusHelper.High(TransactionId);
            response[offset + 1] = ModbusHelper.Low(TransactionId);
            response[offset + 2] = 0;
            response[offset + 3] = 0;
            response[offset + 4] = ModbusHelper.High(Wrapped.ResponseLength);
            response[offset + 5] = ModbusHelper.Low(Wrapped.ResponseLength);
            Wrapped.FillResponse(response, offset + 6, value);
        }

        public override string ToString() {
            return string.Format("[ModbusTCPWrapper Wrapped={0}, TransactionId={1}]", Wrapped, TransactionId);
        }
    }
}
