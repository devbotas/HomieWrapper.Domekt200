﻿using System;
using System.Net.Sockets;
using System.Threading;

namespace SharpModbus
{
    public class ModbusSocketStream
    {
        private readonly TcpClient socket;
        private readonly Action<char, byte[], int> monitor;

        public ModbusSocketStream(TcpClient socket, int timeout, Action<char, byte[], int> monitor = null)
        {
            socket.ReceiveTimeout = timeout;
            socket.SendTimeout = timeout;
            this.monitor = monitor;
            this.socket = socket;
        }

        public void Dispose()
        {
            Tools.Dispose(socket);
        }

        public void Write(byte[] data)
        {
            //implicit discard to allow retry after timeout as per #5
            while (socket.Available > 0) socket.GetStream().ReadByte();
            if (monitor != null) monitor('>', data, data.Length);
            socket.GetStream().Write(data, 0, data.Length);
        }

        public int Read(byte[] data)
        {
            var count = 0;
            var dl = DateTime.Now.AddMilliseconds(socket.ReceiveTimeout);
            while (count < data.Length)
            {
                var available = socket.Available;
                if (available == 0)
                {
                    if (DateTime.Now > dl) break;
                    Thread.Sleep(1);
                }
                else
                {
                    var size = (int)Math.Min(available, data.Length - count);
                    count += socket.GetStream().Read(data, count, size);
                    dl = DateTime.Now; //should come in single packet
                }
            }
            if (monitor != null) monitor('<', data, count);
            return count;
        }
    }
}
