namespace HomieWrapper {
    public interface IExceptionlessSocket {
        /// <summary>
        /// Sends data from <see cref="dataToSend"/> buffer at specified offset and length. Blocks until all data is sent.
        /// </summary>
        /// <returns>Returns false if any failure happens.</returns>
        bool TrySend(byte[] dataToSend, int offset, int length);

        /// <summary>
        /// Blocks until at least <see cref="minimum"/> of bytes are read into provided buffer.
        /// </summary>
        /// <returns>Returns false if any failure happens.</returns>
        (bool IsOk, int Count) TryReceive(byte[] receiveBuffer, int offset, int minimum, int maximum);

        /// <summary>
        /// Disconnects (if needed) and gracefully shuts down connection without throwing any exceptions.
        /// </summary>
        void Dispose();
    }
}
