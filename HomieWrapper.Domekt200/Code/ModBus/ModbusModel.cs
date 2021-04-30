using System.Collections.Generic;

namespace SharpModbus {
    public enum ModbusIoType {
        DI,
        DO,
        WO,
        WI
    }

    public class ModbusModel {
        private readonly IDictionary<string, bool> _digitals = new Dictionary<string, bool>();
        private readonly IDictionary<string, ushort> _words = new Dictionary<string, ushort>();

        public void SetDI(byte slave, ushort address, bool value) {
            var key = Key(ModbusIoType.DI, slave, address);
            _digitals[key] = value;
        }

        public void SetDIs(byte slave, ushort address, bool[] values) {
            for (var i = 0; i < values.Length; i++) {
                var key = Key(ModbusIoType.DI, slave, address + i);
                _digitals[key] = values[i];
            }
        }

        public bool GetDI(byte slave, ushort address) {
            var key = Key(ModbusIoType.DI, slave, address);
            return _digitals[key];
        }

        public bool[] GetDIs(byte slave, ushort address, int count) {
            var values = new bool[count];
            for (var i = 0; i < values.Length; i++) {
                var key = Key(ModbusIoType.DI, slave, address + i);
                values[i] = _digitals[key];
            }
            return values;
        }

        public void SetDO(byte slave, ushort address, bool value) {
            var key = Key(ModbusIoType.DO, slave, address);
            _digitals[key] = value;
        }

        public void SetDOs(byte slave, ushort address, bool[] values) {
            for (var i = 0; i < values.Length; i++) {
                var key = Key(ModbusIoType.DO, slave, address + i);
                _digitals[key] = values[i];
            }
        }

        public bool GetDO(byte slave, ushort address) {
            var key = Key(ModbusIoType.DO, slave, address);
            return _digitals[key];
        }

        public bool[] GetDOs(byte slave, ushort address, int count) {
            var values = new bool[count];
            for (var i = 0; i < values.Length; i++) {
                var key = Key(ModbusIoType.DO, slave, address + i);
                values[i] = _digitals[key];
            }
            return values;
        }

        public void SetWI(byte slave, ushort address, ushort value) {
            var key = Key(ModbusIoType.WI, slave, address);
            _words[key] = value;
        }

        public void SetWIs(byte slave, ushort address, ushort[] values) {
            for (var i = 0; i < values.Length; i++) {
                var key = Key(ModbusIoType.WI, slave, address + i);
                _words[key] = values[i];
            }
        }

        public ushort GetWI(byte slave, ushort address) {
            var key = Key(ModbusIoType.WI, slave, address);
            return _words[key];
        }

        public ushort[] GetWIs(byte slave, ushort address, int count) {
            var values = new ushort[count];
            for (var i = 0; i < values.Length; i++) {
                var key = Key(ModbusIoType.WI, slave, address + i);
                values[i] = _words[key];
            }
            return values;
        }

        public void SetWO(byte slave, ushort address, ushort value) {
            var key = Key(ModbusIoType.WO, slave, address);
            _words[key] = value;
        }

        public void SetWOs(byte slave, ushort address, ushort[] values) {
            for (var i = 0; i < values.Length; i++) {
                var key = Key(ModbusIoType.WO, slave, address + i);
                _words[key] = values[i];
            }
        }

        public ushort GetWO(byte slave, ushort address) {
            var key = Key(ModbusIoType.WO, slave, address);
            return _words[key];
        }

        public ushort[] GetWOs(byte slave, ushort address, int count) {
            var values = new ushort[count];
            for (var i = 0; i < values.Length; i++) {
                var key = Key(ModbusIoType.WO, slave, address + i);
                values[i] = _words[key];
            }
            return values;
        }

        private string Key(ModbusIoType type, byte slave, int address) {
            return string.Format("{0},{1},{2}", slave, type, address);
        }
    }
}
