using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AMWD.Modbus.Tcp.Client;
using DevBot9.Protocols.Homie;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using NLog;

namespace HomieWrapper {
    class Domekt200 {
        private HostDevice _device;

        private IMqttClient _mqttClient;
        private string _mqttClientGuid = Guid.NewGuid().ToString();

        ModbusClient _modbus;
        private object _modbusLock = new object();
        HostStringProperty _actualDateTimeProperty;
        HostEnumProperty _actualState;
        HostEnumProperty _targetState;
        HostEnumProperty _actualVentilationLevelProperty;
        HostFloatProperty _supplyAirTemperatureProperty;

        private DateTime _startTime = DateTime.Now;
        private HostFloatProperty _systemUptime;

        public static Logger Log = LogManager.GetLogger("HomieWrapper.Domekt200");

        public Domekt200() {

        }

        public void Initialize(string mqttBrokerIpAddress, string domektIpAddress) {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder().WithClientId(_mqttClientGuid).WithTcpServer(mqttBrokerIpAddress, 1883).Build();
            _mqttClient.UseApplicationMessageReceivedHandler(e => HandlePublishReceived(e));
            _mqttClient.ConnectAsync(options, CancellationToken.None).Wait();

            _modbus = new ModbusClient(domektIpAddress, 502);

            Log.Info($"Connecting to Modbus device at {domektIpAddress}.");
            _modbus.Connect().Wait();

            Log.Info($"Creating Homie properties.");
            _device = DeviceFactory.CreateHostDevice("recuperator", "Domekt 200");

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
            _device.Initialize((topic, value, qosLevel, isRetained) => {
                _mqttClient.PublishAsync(topic, value, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, isRetained);

            }, topic => {
                _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            });

            // Spinning up spinners.
            Task.Run(async () => await PollDomektOverModbusContinuouslyAsync(new CancellationToken()));
            Task.Run(async () => {
                while (true) {
                    _systemUptime.Value = (float)(DateTime.Now - _startTime).TotalHours;

                    await Task.Delay(5000);
                }
            });
        }

        private void HandlePublishReceived(MqttApplicationMessageReceivedEventArgs e) {
            _device.HandlePublishReceived(e.ApplicationMessage.Topic, Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
        }

        private bool TryReadModbusRegister(KomfoventRegisters register, out int value) {
            var returnResult = false;
            value = 0;

            try {
                lock (_modbusLock) {
                    var registers = _modbus.ReadHoldingRegisters(2, (ushort)((ushort)register - 1), 1).Result;

                    if (registers != null) { value = registers[0].RegisterValue; }
                }
                returnResult = true;
            }
            catch (Exception ex) {
                Log.Warn($"Could not read ModBus register {register}, because of {ex.Message}.");
            }

            return returnResult;
        }
        private bool TryWriteModbusRegister(KomfoventRegisters register, int value) {
            var returnResult = false;

            try {
                lock (_modbusLock) {
                    var pyzda = _modbus.WriteSingleRegister(2, new AMWD.Modbus.Common.Structures.ModbusObject() { Address = (ushort)((ushort)register - 1), RegisterValue = (ushort)value, Type = AMWD.Modbus.Common.ModbusObjectType.HoldingRegister }).Result;
                }
            }
            catch (Exception ex) {
                Log.Warn($"Could not write ModBus register {register}, because of {ex.Message}.");
            }

            return returnResult;
        }
        private bool TryReadDateTime(out DateTime dateTime) {
            var succeeded = false;

            TryReadModbusRegister(KomfoventRegisters.HourAndMinute, out var hourAndMinute);
            var hour = hourAndMinute >> 8;
            var minute = hourAndMinute & 0xF;

            TryReadModbusRegister(KomfoventRegisters.MonthAndDay, out var monthAndDay);
            var month = monthAndDay >> 8;
            var day = monthAndDay & 0xFF;

            TryReadModbusRegister(KomfoventRegisters.Year, out var year);

            try {
                dateTime = new DateTime(year, month, day, hour, minute, 0);
                succeeded = true;
            }
            catch {
                // Sometime conversion crashes because Domekt return some insane value. Probably due to some internal race conditions.
                dateTime = DateTime.Now;
                succeeded = false;
            }

            return succeeded;
        }
        private async Task PollDomektOverModbusContinuouslyAsync(CancellationToken cancellationToken) {
            Log.Info($"Spinning up parameter monitoring task.");
            while (true) {
                try {

                    var allOk = TryReadModbusRegister(KomfoventRegisters.StartStop, out var startStopStatus);
                    if (allOk) {
                        //
                    }

                    allOk = TryReadDateTime(out var dateTime);
                    if (allOk) {
                        _actualDateTimeProperty.Value = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                    }
                    else {
                        Log.Warn("Failed to parse read date.");
                        _modbus.Disconnect().Wait();
                        _modbus.Connect().Wait();
                    }

                    allOk = TryReadModbusRegister(KomfoventRegisters.VentilationLevelManual, out var ventilationLevelManual);
                    if (allOk) {
                        //
                    }

                    allOk = TryReadModbusRegister(KomfoventRegisters.VentilationLevelCurrent, out var ventilationLevelCurrent);
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

                    allOk = TryReadModbusRegister(KomfoventRegisters.ModeAutoManual, out var ventilationMode);
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

                    allOk = TryReadModbusRegister(KomfoventRegisters.SupplyAirTemperature, out var supplyAirTemperature);
                    if (allOk) {
                        _supplyAirTemperatureProperty.Value = supplyAirTemperature / 10.0f;
                    }
                }
                catch (IOException ex) {
                    Log.Error($"Reading registers failed, because: {ex.Message}");

                    // I think Domekt Ping module has serious mutex issues. Using it from the web and over RS485 simulteneously results in reboot?.. Trying to reconnect in such case.
                    Log.Error($"Reconnecting...");
                    _modbus.Disconnect().Wait();
                    _modbus.Connect().Wait();
                }

                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
