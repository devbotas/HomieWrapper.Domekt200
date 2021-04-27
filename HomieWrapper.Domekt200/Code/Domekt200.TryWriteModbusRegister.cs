using System;

namespace HomieWrapper {
    partial class Domekt200 {
        private bool TryWriteModbusRegister(KomfoventRegisters register, int value) {
            var returnResult = false;

            try {
                lock (_modbusLock) {
                    _modbus.WriteSingleRegister(2, new AMWD.Modbus.Common.Structures.ModbusObject() { Address = (ushort)((ushort)register - 1), RegisterValue = (ushort)value, Type = AMWD.Modbus.Common.ModbusObjectType.HoldingRegister });
                }
            }
            catch (Exception ex) {
                Log.Warn($"Could not write ModBus register {register}, because of {ex.Message}.");
            }

            return returnResult;
        }
    }
}
