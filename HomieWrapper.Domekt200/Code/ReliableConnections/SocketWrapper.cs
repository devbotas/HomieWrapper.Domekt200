using System;
using System.Net.Sockets;

namespace HomieWrapper {
    class TcpSocketWrapper : IExceptionlessSocket {
        public Socket ActualSocket;
        public Exception LastException;

        public TcpSocketWrapper(Socket actualTcpSocket) {
            ActualSocket = actualTcpSocket;
        }

        public bool TrySend(byte[] dataToSend, int offset, int length) {
            var actualSocket = ActualSocket;
            if (actualSocket == null) return false;

            var numberOfBytesSent = 0;
            var isOk = true;

            try {
                while (numberOfBytesSent != length) {
                    numberOfBytesSent += actualSocket.Send(dataToSend, offset + numberOfBytesSent, length - numberOfBytesSent, SocketFlags.None);
                }
            }
            catch (SocketException ex) {
                LastException = ex;
                isOk = false;
            }
            catch (ObjectDisposedException ex) {
                LastException = ex;
                isOk = false;
            }

            return isOk;
        }

        public (bool IsOk, int Count) TryReceive(byte[] receiveBuffer, int offset, int minimum, int maximum) {
            var actualSocket = ActualSocket;
            if (actualSocket == null) return (false, 0);

            var isOk = true;
            var numberOfBytesReceived = 0;

            try {
                var available = actualSocket.Available;
                var bytesToReceive = minimum;

                if (available > minimum) {
                    if (available > maximum) bytesToReceive = maximum;
                    else bytesToReceive = available;
                }

                while (numberOfBytesReceived != bytesToReceive) {
                    var received = actualSocket.Receive(receiveBuffer, offset + numberOfBytesReceived, bytesToReceive - numberOfBytesReceived, SocketFlags.None);

                    if (received == 0) {
                        // Socket.Receive(...) will return zero bytes if client closes connection. Otherwise it will block until some of the bytes are available.
                        isOk = false;
                        break;
                    }

                    numberOfBytesReceived += received;
                }
            }
            catch (SocketException ex) {
                LastException = ex;
                isOk = false;
            }
            catch (ObjectDisposedException ex) {
                LastException = ex;
                isOk = false;
            }

            return (isOk, numberOfBytesReceived);
        }

        public void Dispose() {
            var actualSocket = ActualSocket;
            if (actualSocket == null) return;

            try {
                actualSocket.Shutdown(SocketShutdown.Both);
                actualSocket.Close();
            }
            catch (SocketException ex) {
                LastException = ex;
            }
            catch (ObjectDisposedException ex) {
                LastException = ex;
            }

            try {
                actualSocket.Dispose();
            }
            catch (SocketException ex) {
                LastException = ex;
            }
            catch (ObjectDisposedException ex) {
                LastException = ex;
            }

            ActualSocket = null;
        }
    }
}
