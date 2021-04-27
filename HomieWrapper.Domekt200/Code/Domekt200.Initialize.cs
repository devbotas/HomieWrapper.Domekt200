﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AMWD.Modbus.Tcp.Client;
using DevBot9.Protocols.Homie;

namespace HomieWrapper {
    partial class Domekt200 {
        public void Initialize(ReliableBroker reliableBroker, string domektIpAddress) {
            _reliableBroker = reliableBroker;

            _modbus = new ModbusClient(domektIpAddress, 502);

            Log.Info($"Connecting to Modbus device at {domektIpAddress}.");
            _modbus.Connect().Wait();

            Log.Info($"Creating Homie properties.");
            _device = DeviceFactory.CreateHostDevice("recuperator", "Domekt 200");
            _reliableBroker.PublishReceived += _device.HandlePublishReceived;

            // General section.
            _device.UpdateNodeInfo("general", "General information", "no-type");
            _actualState = _device.CreateHostEnumProperty(PropertyType.State, "general", "actual-state", "Actual state", new[] { "UNKNOWN", "OFF", "STARTING", "ON-AUTO", "ON-LOW", "ON-MEDIUM", "ON-HIGH" }, "OFF");
            _targetState = _device.CreateHostEnumProperty(PropertyType.Command, "general", "target-state", "Target state", new[] { "OFF", "ON-AUTO", "ON-LOW", "ON-MEDIUM", "ON-HIGH" }, "OFF");
            _targetState.PropertyChanged += (sender, e) => {
                switch (_targetState.Value) {
                    case "OFF":
                        TryWriteModbusRegister(KomfoventRegisters.StartStop, 0);
                        break;

                    case "ON-AUTO":
                        if (_actualState.Value == "OFF") {
                            TryWriteModbusRegister(KomfoventRegisters.StartStop, 1);
                            Thread.Sleep(100);
                        }
                        TryWriteModbusRegister(KomfoventRegisters.ModeAutoManual, 1);

                        break;

                    case "ON-LOW":
                        if (_actualState.Value == "OFF") {
                            TryWriteModbusRegister(KomfoventRegisters.StartStop, 1);
                            Thread.Sleep(100);
                        }

                        TryWriteModbusRegister(KomfoventRegisters.ModeAutoManual, 0);
                        Thread.Sleep(100);
                        TryWriteModbusRegister(KomfoventRegisters.VentilationLevelManual, 1);
                        break;

                    case "ON-MEDIUM":
                        if (_actualState.Value == "OFF") {
                            TryWriteModbusRegister(KomfoventRegisters.StartStop, 1);
                            Thread.Sleep(100);
                        }
                        TryWriteModbusRegister(KomfoventRegisters.ModeAutoManual, 0);
                        Thread.Sleep(100);
                        TryWriteModbusRegister(KomfoventRegisters.VentilationLevelManual, 2);
                        break;

                    case "ON-HIGH":
                        if (_actualState.Value == "OFF") {
                            TryWriteModbusRegister(KomfoventRegisters.StartStop, 1);
                            Thread.Sleep(100);
                        }
                        TryWriteModbusRegister(KomfoventRegisters.ModeAutoManual, 0);
                        Thread.Sleep(100);
                        TryWriteModbusRegister(KomfoventRegisters.VentilationLevelManual, 3);
                        break;
                }
            };

            // Ventilation section.
            _device.UpdateNodeInfo("ventilation", "Ventilation related properties", "no-type");
            _actualVentilationLevelProperty = _device.CreateHostEnumProperty(PropertyType.State, "ventilation", "actual-level", "Actual level", new[] { "OFF", "LOW", "MEDIUM", "HIGH" });

            // Temperatures section.
            _device.UpdateNodeInfo("temperatures", "Various temperatures", "no-type");
            _supplyAirTemperatureProperty = _device.CreateHostFloatProperty(PropertyType.State, "temperatures", "supply-air-temperature", "Supply air temperature", 16, "°C");

            // System section.
            _device.UpdateNodeInfo("system", "System", "no-type");
            _actualDateTimeProperty = _device.CreateHostStringProperty(PropertyType.State, "system", "date-time", "Current date ant time.", "");
            _systemUptime = _device.CreateHostFloatProperty(PropertyType.State, "system", "uptime", "Uptime", 0, "h");

            // Now starting up everything.
            Log.Info($"Initializing Homie entities.");
            _device.Initialize(_reliableBroker.PublishToTopic, _reliableBroker.SubscribeToTopic);

            // Spinning up spinners.
            Task.Run(async () => await PollDomektOverModbusContinuouslyAsync(new CancellationToken()));
            Task.Run(async () => {
                while (true) {
                    _systemUptime.Value = (float)(DateTime.Now - _startTime).TotalHours;

                    await Task.Delay(5000);
                }
            });
        }
    }
}
