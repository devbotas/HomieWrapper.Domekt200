﻿using System;

namespace SharpModbus {
    public class ModbusMaster : IDisposable {
        public delegate void WriteToDeviceDelegate(byte[] payload);
        public delegate int ReadFromDeviceDelegate(byte[] payload);

        public WriteToDeviceDelegate WriteToDevice;
        public ReadFromDeviceDelegate ReadFromDevice;

        public static ModbusMaster TCP(string ip, int port, int timeout = 400) {
            var socket = Tools.ConnectWithTimeout(ip, port, timeout);
            var stream = new ModbusSocketStream(socket, timeout);
            var protocol = new ModbusTCPProtocol();
            return new ModbusMaster(stream, protocol);
        }

        private readonly ModbusTCPProtocol protocol;
        // private readonly ModbusSocketStream stream;

        public ModbusMaster(ModbusSocketStream stream, ModbusTCPProtocol protocol) {
            // this.stream = stream;
            this.protocol = protocol;
        }

        public void Dispose() {
            // Tools.Dispose(stream);
        }

        public bool ReadCoil(byte slave, ushort address) {
            return ReadCoils(slave, address, 1)[0]; //there is no code for single read
        }

        public bool ReadInput(byte slave, ushort address) {
            return ReadInputs(slave, address, 1)[0]; //there is no code for single read
        }

        public ushort ReadInputRegister(byte slave, ushort address) {
            return ReadInputRegisters(slave, address, 1)[0]; //there is no code for single read
        }

        public ushort ReadHoldingRegister(byte slave, ushort address) {
            return ReadHoldingRegisters(slave, address, 1)[0]; //there is no code for single read
        }

        public bool[] ReadCoils(byte slave, ushort address, ushort count) {
            return Execute(new ModbusF01ReadCoils(slave, address, count)) as bool[];
        }

        public bool[] ReadInputs(byte slave, ushort address, ushort count) {
            return Execute(new ModbusF02ReadInputs(slave, address, count)) as bool[];
        }

        public ushort[] ReadInputRegisters(byte slave, ushort address, ushort count) {
            return Execute(new ModbusF04ReadInputRegisters(slave, address, count)) as ushort[];
        }

        public ushort[] ReadHoldingRegisters(byte slave, ushort address, ushort count) {
            return Execute(new ModbusF03ReadHoldingRegisters(slave, address, count)) as ushort[];
        }

        public void WriteCoil(byte slave, ushort address, bool value) {
            Execute(new ModbusF05WriteCoil(slave, address, value));
        }

        public void WriteRegister(byte slave, ushort address, ushort value) {
            Execute(new ModbusF06WriteRegister(slave, address, value));
        }

        public void WriteCoils(byte slave, ushort address, params bool[] values) {
            Execute(new ModbusF15WriteCoils(slave, address, values));
        }

        public void WriteRegisters(byte slave, ushort address, params ushort[] values) {
            Execute(new ModbusF16WriteRegisters(slave, address, values));
        }

        private object Execute(IModbusCommand cmd) {
            var wrapper = protocol.Wrap(cmd);
            var request = new byte[wrapper.RequestLength];
            var response = new byte[wrapper.ResponseLength];
            wrapper.FillRequest(request, 0);
            WriteToDevice(request);
            var count = ReadFromDevice(response);
            if (count < response.Length) wrapper.CheckException(response, count);
            return wrapper.ParseResponse(response, 0);
        }
    }
}
