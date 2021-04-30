using System;

namespace SharpModbus {
    public class ModbusException : Exception {
        public byte Code { get; }

        public ModbusException(byte code) :
            base(string.Format("Modbus exception {0}", code)) {
            this.Code = code;
        }
    }
}
