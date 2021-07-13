using System;
using System.Threading;
using System.Threading.Tasks;
using DevBot9.Protocols.Homie;

namespace HomieWrapper {
    partial class Domekt200 {
        public void Initialize(string brokerIp, string modBusIp) {
            Log.Info($"Creating Homie properties.");
            _device = DeviceFactory.CreateHostDevice("recuperator", "Domekt 200");

            // General section.
            _device.UpdateNodeInfo("general", "General information", "no-type");
            _actualState = _device.CreateHostChoiceProperty(PropertyType.State, "general", "actual-state", "Actual state", new[] { "UNKNOWN", "OFF", "STARTING", "ON-AUTO", "ON-LOW", "ON-MEDIUM", "ON-HIGH" }, "OFF");
            _targetState = _device.CreateHostChoiceProperty(PropertyType.Command, "general", "target-state", "Target state", new[] { "OFF", "ON-AUTO", "ON-LOW", "ON-MEDIUM", "ON-HIGH" }, "OFF");
            _targetState.PropertyChanged += (sender, e) => {
                switch (_targetState.Value) {
                    case "OFF":
                        _reliableModbus.TryWriteModbusRegister(KomfoventRegisters.StartStop, 0);
                        break;

                    case "ON-AUTO":
                        if (_actualState.Value == "OFF") {
                            _reliableModbus.TryWriteModbusRegister(KomfoventRegisters.StartStop, 1);
                            Thread.Sleep(100);
                        }
                        _reliableModbus.TryWriteModbusRegister(KomfoventRegisters.ModeAutoManual, 1);

                        break;

                    case "ON-LOW":
                        if (_actualState.Value == "OFF") {
                            _reliableModbus.TryWriteModbusRegister(KomfoventRegisters.StartStop, 1);
                            Thread.Sleep(100);
                        }

                        _reliableModbus.TryWriteModbusRegister(KomfoventRegisters.ModeAutoManual, 0);
                        Thread.Sleep(100);
                        _reliableModbus.TryWriteModbusRegister(KomfoventRegisters.VentilationLevelManual, 1);
                        break;

                    case "ON-MEDIUM":
                        if (_actualState.Value == "OFF") {
                            _reliableModbus.TryWriteModbusRegister(KomfoventRegisters.StartStop, 1);
                            Thread.Sleep(100);
                        }
                        _reliableModbus.TryWriteModbusRegister(KomfoventRegisters.ModeAutoManual, 0);
                        Thread.Sleep(100);
                        _reliableModbus.TryWriteModbusRegister(KomfoventRegisters.VentilationLevelManual, 2);
                        break;

                    case "ON-HIGH":
                        if (_actualState.Value == "OFF") {
                            _reliableModbus.TryWriteModbusRegister(KomfoventRegisters.StartStop, 1);
                            Thread.Sleep(100);
                        }
                        _reliableModbus.TryWriteModbusRegister(KomfoventRegisters.ModeAutoManual, 0);
                        Thread.Sleep(100);
                        _reliableModbus.TryWriteModbusRegister(KomfoventRegisters.VentilationLevelManual, 3);
                        break;
                }
            };
            _actualModbusConnectionState = _device.CreateHostChoiceProperty(PropertyType.State, "general", "modbus-state", "Modbus state", new[] { "OK", "DISCONNECTED" });

            // Ventilation section.
            _device.UpdateNodeInfo("ventilation", "Ventilation related properties", "no-type");
            _actualVentilationLevelProperty = _device.CreateHostChoiceProperty(PropertyType.State, "ventilation", "actual-level", "Actual level", new[] { "OFF", "LOW", "MEDIUM", "HIGH" });

            // Temperatures section.
            _device.UpdateNodeInfo("temperatures", "Various temperatures", "no-type");
            _supplyAirTemperatureProperty = _device.CreateHostNumberProperty(PropertyType.State, "temperatures", "supply-air-temperature", "Supply air temperature", 16, "°C");

            // System section.
            _device.UpdateNodeInfo("system", "System", "no-type");
            _actualDateTimeProperty = _device.CreateHostTextProperty(PropertyType.State, "system", "date-time", "Current date ant time.", "");
            _systemUptime = _device.CreateHostNumberProperty(PropertyType.State, "system", "uptime", "Uptime", 0, "h");
            _disconnectCount = _device.CreateHostNumberProperty(PropertyType.State, "system", "disconnect-count", "Modbus disconnect count", decimalPlaces: 0);

            // Now starting up everything.
            Log.Info($"Initializing Homie entities.");
            _reliableModbus.Initialize(modBusIp);
            _broker.Initialize(brokerIp, (severity, message) => {
                if (severity == "Info") { Log.Info(message); }
                else if (severity == "Error") { Log.Error(message); }
                else { Log.Debug(message); }
            });
            _device.Initialize(_broker, (severity, message) => {
                if (severity == "Info") { Log.Info(message); }
                else if (severity == "Error") { Log.Error(message); }
                else { Log.Debug(message); }
            });

            // Spinning up spinners.
            Task.Run(async () => await PollDomektOverModbusContinuouslyAsync(new CancellationToken()));
            Task.Run(async () => {
                while (true) {
                    _systemUptime.Value = (float)(DateTime.Now - _startTime).TotalHours;

                    await Task.Delay(5000);
                }
            });
            Task.Run(async () => {
                var cachedState = true;
                while (true) {

                    if ((_reliableModbus.IsConnected == false) && (cachedState == true)) {
                        _actualModbusConnectionState.Value = "DISCONNECTED";
                        _disconnectCount.Value = _reliableModbus.DisconnectCount;
                    }

                    if ((_reliableModbus.IsConnected == true) && (cachedState == false)) {
                        _actualModbusConnectionState.Value = "OK";
                    }

                    cachedState = _reliableModbus.IsConnected;

                    await Task.Delay(10);
                }
            });
        }
    }
}
