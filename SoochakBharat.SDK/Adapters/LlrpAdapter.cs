using System;
using System.Threading.Tasks;
using SoochakBharat.SDK.Events;
using SoochakBharat.SDK.Models.Options;

namespace SoochakBharat.SDK.Adapters
{
    public class LlrpAdapter : IReaderAdapter
    {
        public event EventHandler<TagReadEventArgs>? TagRead;
        public event EventHandler<ReaderStatusEventArgs>? StatusChanged;

        public async Task<bool> ConnectAsync(ReaderConnectionOptions options)
        {
            // Not implemented yet — return false so factory continues fallback logic
            StatusChanged?.Invoke(this, new ReaderStatusEventArgs("LLRP", ReaderConnectionState.Error, "LLRP Not Implemented Yet"));
            return false;
        }

        public Task<bool> DisconnectAsync() => Task.FromResult(true);
        public Task<bool> StartInventoryAsync(InventoryOptions? options = null) => Task.FromResult(true);
        public Task<bool> StopInventoryAsync() => Task.FromResult(true);

        public void Dispose() { }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

    }
}
