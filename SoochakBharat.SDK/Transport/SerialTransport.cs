using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace SoochakBharat.SDK.Transport
{
    public class SerialTransport : ITransport
    {
        private SerialPort? _port;

        public bool IsConnected => _port?.IsOpen ?? false;

        public async Task ConnectAsync(string address, int baudRate)
        {
            _port = new SerialPort(address, baudRate)
            {
                ReadTimeout = 500,
                WriteTimeout = 500
            };

            _port.Open();
            await Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            _port?.Close();
            return Task.CompletedTask;
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct = default)
        {
            if (_port == null || !_port.IsOpen) return Task.FromResult(0);

            int bytes = _port.Read(buffer, offset, count);
            return Task.FromResult(bytes);
        }

        public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct = default)
        {
            if (_port == null || !_port.IsOpen) return Task.CompletedTask;

            _port.Write(buffer, offset, count);
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            _port?.Dispose();
            return ValueTask.CompletedTask;
        }

        public void Dispose() => _port?.Dispose();
    }
}
