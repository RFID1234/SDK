using System;
using System.Threading.Tasks;
using SoochakBharat.SDK.Models.Options;
using SoochakBharat.SDK.Events;

namespace SoochakBharat.SDK.Adapters
{
    public interface IReaderAdapter : IDisposable, IAsyncDisposable
    {
        Task<bool> ConnectAsync(ReaderConnectionOptions options);
        Task<bool> DisconnectAsync();

        Task<bool> StartInventoryAsync(InventoryOptions? options = null);
        Task<bool> StopInventoryAsync();

        event EventHandler<TagReadEventArgs>? TagRead;
        event EventHandler<ReaderStatusEventArgs>? StatusChanged;
    }
}
