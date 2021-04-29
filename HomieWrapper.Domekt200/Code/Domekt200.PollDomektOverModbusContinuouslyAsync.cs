using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HomieWrapper {
    partial class Domekt200 {
        private async Task PollDomektOverModbusContinuouslyAsync(CancellationToken cancellationToken) {
            Log.Info($"Spinning up parameter monitoring task.");
            while (true) {
                if (_reliableModbus.IsConnected == false) continue;

                try {
                    var allOk = _reliableModbus.TryReadModbusRegister(KomfoventRegisters.StartStop, out var startStopStatus);
                    if (allOk) {
                        //
                    }

                    allOk = TryReadDateTime(out var dateTime);
                    if (allOk) {
                        _actualDateTimeProperty.Value = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                    }
                    else {
                        Log.Warn("Failed to parse read date.");
                        //_modbus.Disconnect().Wait();
                        //_modbus.Connect().Wait();
                    }

                    allOk = _reliableModbus.TryReadModbusRegister(KomfoventRegisters.VentilationLevelManual, out var ventilationLevelManual);
                    if (allOk) {
                        //
                    }

                    allOk = _reliableModbus.TryReadModbusRegister(KomfoventRegisters.VentilationLevelCurrent, out var ventilationLevelCurrent);
                    if (allOk) {
                        switch (ventilationLevelCurrent) {
                            case 0:
                                _actualVentilationLevelProperty.Value = "OFF";
                                break;

                            case 1:
                                _actualVentilationLevelProperty.Value = "LOW";
                                break;

                            case 2:
                                _actualVentilationLevelProperty.Value = "MEDIUM";
                                break;

                            case 3:
                                _actualVentilationLevelProperty.Value = "HIGH";
                                break;
                        }
                    }

                    allOk = _reliableModbus.TryReadModbusRegister(KomfoventRegisters.ModeAutoManual, out var ventilationMode);
                    if (allOk) {
                        if (ventilationMode == 0) { _actualVentilationLevelProperty.Value = "MANUAL"; }
                        if (ventilationMode == 1) { _actualVentilationLevelProperty.Value = "AUTOMATIC"; }
                    }

                    if (allOk) {
                        if (startStopStatus == 0) { _actualState.Value = "OFF"; }
                        else if ((startStopStatus == 1) && (ventilationMode == 0)) {
                            switch (ventilationLevelCurrent) {
                                case 1:
                                    _actualState.Value = "ON-LOW";
                                    break;

                                case 2:
                                    _actualState.Value = "ON-MEDIUM";
                                    break;

                                case 3:
                                    _actualState.Value = "ON-HIGH";
                                    break;
                            }
                        }
                        else if ((startStopStatus == 1) && (ventilationMode == 1)) {
                            _actualState.Value = "ON-AUTO";
                        }
                        else {
                            _actualState.Value = "UNKNOWN";
                        }
                    }

                    allOk = _reliableModbus.TryReadModbusRegister(KomfoventRegisters.SupplyAirTemperature, out var supplyAirTemperature);
                    if (allOk) {
                        _supplyAirTemperatureProperty.Value = supplyAirTemperature / 10.0f;
                    }
                }
                catch (IOException ex) {
                    Log.Error($"Reading registers failed, because: {ex.Message}");

                    // I think Domekt Ping module has serious mutex issues. Using it from the web and over RS485 simulteneously results in reboot?.. Trying to reconnect in such case.
                    // Log.Error($"Reconnecting...");
                    //_modbus.Disconnect().Wait();
                    //_modbus.Connect().Wait();
                }
                catch (Exception ex) {
                    var bybis = 1;
                }

                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
