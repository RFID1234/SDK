using System.Threading;
using System.Threading.Tasks;

namespace SoochakBharat.SDK.Transport
{
    public interface ITransport : IAsyncDisposable, System.IDisposable
    {
        bool IsConnected { get; }
        Task ConnectAsync(string host, int port);
        Task DisconnectAsync();
        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct);
        Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct);
    }
}
