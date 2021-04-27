using System;
using System.Threading.Tasks;

namespace HomieWrapper {
    partial class Domekt200 {
        private bool TryReadModbusRegister(KomfoventRegisters register, out int value) {
            var returnResult = false;
            value = 0;

            try {
                lock (_modbusLock) {

                    ushort taskReturnValue = 0;
                    Task.Run((async () => {
                        var registers = await _modbus.ReadHoldingRegisters(2, (ushort)((ushort)register - 1), 1);
                        if (registers != null) {
                            if (registers.Count == 0) { var pzdc = 1; }

                            taskReturnValue = registers[0].RegisterValue;
                        }
                        await Task.Delay(100);
                    })).Wait();

                    value = taskReturnValue;
                }
                returnResult = true;
            }
            catch (Exception ex) {
                Log.Warn($"Could not read ModBus register {register}, because of {ex.Message}.");
            }

            return returnResult;
        }
    }
}
