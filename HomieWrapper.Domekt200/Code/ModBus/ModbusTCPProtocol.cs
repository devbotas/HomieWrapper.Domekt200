﻿namespace SharpModbus {
    public class ModbusTCPProtocol {
        public ushort TransactionId { get; set; } = 0;

        public ModbusTCPWrapper Wrap(IModbusCommand wrapped) {
            return new ModbusTCPWrapper(wrapped, TransactionId++);
        }

        public ModbusTCPWrapper Parse(byte[] request, int offset) {
            var wrapped = ModbusParser.Parse(request, offset + 6);
            Tools.AssertEqual(wrapped.RequestLength, ModbusHelper.GetUShort(request, offset + 4),
                "RequestLength mismatch got {0} expected {1}");
            var transaction = ModbusHelper.GetUShort(request, offset);
            return new ModbusTCPWrapper(wrapped, transaction);
        }
    }
}
