namespace SharpModbus {
    public class ModbusTCPWrapper {
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

        public override string ToString() {
            return string.Format("[ModbusTCPWrapper Wrapped={0}, TransactionId={1}]", Wrapped, TransactionId);
        }
    }
}
