using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SoochakBharat.SDK.Transport
{
    public class TcpTransport : ITransport
    {
        private TcpClient? _client;
        private NetworkStream? _stream;

        public bool IsConnected => _client?.Connected ?? false;

        public async Task ConnectAsync(string address, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(address, port);
            _stream = _client.GetStream();
        }

        public Task DisconnectAsync()
        {
            _client?.Close();
            return Task.CompletedTask;
        }

        public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct = default)
        {
            if (_stream == null) return 0;
            return await _stream.ReadAsync(buffer.AsMemory(offset, count), ct);
        }

        public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct = default)
        {
            if (_stream == null) return;
            await _stream.WriteAsync(buffer.AsMemory(offset, count), ct);
        }

        public ValueTask DisposeAsync()
        {
            _client?.Dispose();
            return ValueTask.CompletedTask;
        }

        public void Dispose() => _client?.Dispose();
    }
}
