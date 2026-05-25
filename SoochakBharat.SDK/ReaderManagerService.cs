using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SoochakBharat.SDK.Concurrency;
using SoochakBharat.SDK.Events;
using SoochakBharat.SDK.Models;

namespace SoochakBharat.SDK
{
    /// <summary>
    /// Default implementation of <see cref="IReaderManager"/> for managing multiple adapters.
    /// Thread-safe, designed for UI + LocalApiServer consumption.
    /// </summary>
    public class ReaderManagerService : IReaderManager
    {
        private readonly ConcurrentDictionary<string, Adapters.IReaderAdapter> _adapters = new();
        private readonly ConcurrentDictionary<string, TagCache> _tagCaches = new();
        private readonly ConcurrentDictionary<string, InventoryStats> _stats = new();
        private readonly object _statsLock = new();

        public event EventHandler<ReaderStatusEventArgs>? ReaderStatusChanged;
        public event EventHandler<TagReadEventArgs>? TagRead;

        public async Task<string> AddReaderAsync(string id, Adapters.IReaderAdapter adapter)
        {
            if (string.IsNullOrWhiteSpace(id))
                id = Guid.NewGuid().ToString("N");

            if (_adapters.TryAdd(id, adapter))
            {
                _tagCaches.TryAdd(id, new TagCache());
                _stats.TryAdd(id, new InventoryStats { ReaderId = id });

                adapter.StatusChanged += OnAdapterStatusChanged;
                adapter.TagRead += OnAdapterTagRead;
            }

            return id;
        }

        public async Task<bool> RemoveReaderAsync(string id)
        {
            if (_adapters.TryRemove(id, out var adapter))
            {
                adapter.StatusChanged -= OnAdapterStatusChanged;
                adapter.TagRead -= OnAdapterTagRead;
                await adapter.DisconnectAsync();
                await adapter.DisposeAsync();
                _tagCaches.TryRemove(id, out _);
                _stats.TryRemove(id, out _);
                return true;
            }

            return false;
        }

        public async Task<bool> StartInventoryAsync(string id)
        {
            if (!_adapters.TryGetValue(id, out var adapter))
                return false;

            _tagCaches.GetOrAdd(id, _ => new TagCache()).Clear();
            lock (_statsLock)
            {
                _stats[id] = new InventoryStats
                {
                    ReaderId = id,
                    Uptime = TimeSpan.Zero,
                    TagsPerSecond = 0,
                    AverageRssi = 0,
                    ActiveAntennaCount = 0,
                    UniqueTagCount = 0,
                    TotalReadEvents = 0
                };
            }

            return await adapter.StartInventoryAsync();
        }

        public async Task<bool> StopInventoryAsync(string id)
        {
            if (!_adapters.TryGetValue(id, out var adapter))
                return false;

            return await adapter.StopInventoryAsync();
        }

        public IReadOnlyCollection<ReaderInfo> ListReaders()
        {
            return _adapters.Keys
                .Select(id => new ReaderInfo
                {
                    Id = id,
                    Address = id,
                    IsConnected = true
                })
                .ToArray();
        }

        public IReadOnlyCollection<TagData> GetTags(string id)
        {
            if (_tagCaches.TryGetValue(id, out var cache))
                return cache.Snapshot();

            return Array.Empty<TagData>();
        }

        public InventoryStats GetStats(string id)
        {
            if (_stats.TryGetValue(id, out var stats))
                return stats;

            return new InventoryStats { ReaderId = id };
        }

        private void OnAdapterStatusChanged(object? sender, ReaderStatusEventArgs e)
        {
            ReaderStatusChanged?.Invoke(this, e);
        }

        private void OnAdapterTagRead(object? sender, TagReadEventArgs e)
        {
            var readerId = e.ReaderId ?? "default";
            var cache = _tagCaches.GetOrAdd(readerId, _ => new TagCache());
            var updated = cache.Upsert(e.Tag);

            lock (_statsLock)
            {
                var s = _stats.GetOrAdd(readerId, _ => new InventoryStats { ReaderId = readerId });
                s.TotalReadEvents += e.Tag.ReadCount;
                s.UniqueTagCount = cache.Snapshot().Count;
                // TagsPerSecond and AverageRssi can be computed client-side with more context; keep simple rolling averages here.
                s.AverageRssi = s.AverageRssi == 0
                    ? updated.Rssi
                    : (s.AverageRssi * 0.9 + updated.Rssi * 0.1);
                _stats[readerId] = s;
            }

            TagRead?.Invoke(this, e);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var kvp in _adapters)
            {
                try
                {
                    await kvp.Value.DisconnectAsync();
                    await kvp.Value.DisposeAsync();
                }
                catch { }
            }

            _adapters.Clear();
            _tagCaches.Clear();
            _stats.Clear();
        }
    }
}


