using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoochakBharat.SDK.Events;
using SoochakBharat.SDK.Models;

namespace SoochakBharat.SDK
{
    /// <summary>
    /// Abstraction for managing one or more reader adapters in a thread-safe way.
    /// </summary>
    public interface IReaderManager : IAsyncDisposable
    {
        event EventHandler<ReaderStatusEventArgs>? ReaderStatusChanged;
        event EventHandler<TagReadEventArgs>? TagRead;

        Task<string> AddReaderAsync(string id, Adapters.IReaderAdapter adapter);
        Task<bool> RemoveReaderAsync(string id);

        Task<bool> StartInventoryAsync(string id);
        Task<bool> StopInventoryAsync(string id);

        IReadOnlyCollection<ReaderInfo> ListReaders();
        IReadOnlyCollection<TagData> GetTags(string id);
        InventoryStats GetStats(string id);
    }
}


