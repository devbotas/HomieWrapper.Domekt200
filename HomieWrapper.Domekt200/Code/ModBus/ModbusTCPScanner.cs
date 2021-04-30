using System.Collections.Generic;

namespace SharpModbus {
    public class ModbusTCPScanner {
        private readonly List<byte> _buffer = new();

        public void Append(byte[] data, int offset, int count) {
            for (var i = 0; i < count; i++) _buffer.Add(data[offset + i]);
        }

        public ModbusTCPWrapper Scan() {
            if (_buffer.Count >= 6) {
                var length = ModbusHelper.GetUShort(_buffer[4], _buffer[5]);
                if (_buffer.Count >= 6 + length) {
                    var request = _buffer.GetRange(0, 6 + length).ToArray();
                    _buffer.RemoveRange(0, 6 + length);
                    return Parse(request, 0);
                }
            }
            return null; //not enough data to parse
        }

        private ModbusTCPWrapper Parse(byte[] request, int offset) {
            var wrapped = ModbusParser.Parse(request, offset + 6);
            Tools.AssertEqual(wrapped.RequestLength, ModbusHelper.GetUShort(request, offset + 4), "RequestLength mismatch got {0} expected {1}");
            var transaction = ModbusHelper.GetUShort(request, offset);
            return new ModbusTCPWrapper(wrapped, transaction);
        }
    }
}
