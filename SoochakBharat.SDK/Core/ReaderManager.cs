using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SoochakBharat.SDK.Adapters;
using SoochakBharat.SDK.Events;
using SoochakBharat.SDK.Models;
using SoochakBharat.SDK.Models.Options;
using SoochakBharat.SDK.Utils;

namespace SoochakBharat.SDK.Core
{
    public class ReaderManager : IAsyncDisposable, IDisposable
    {
        private readonly ConcurrentDictionary<string, IReaderAdapter> _adapters = new();
        private readonly ConcurrentDictionary<string, DateTime> _dedup = new();
        private readonly TimeSpan _dedupTtl;
        private readonly SemaphoreSlim _startStopLock = new(1, 1);
        private bool _disposed;

        public event EventHandler<TagReadEventArgs>? TagRead;
        public event EventHandler<ReaderStatusEventArgs>? ReaderStatusChanged;

        public ReaderManager(TimeSpan? dedupTtl = null) { _dedupTtl = dedupTtl ?? TimeSpan.FromSeconds(5); }

        public static ReaderManager FromConfigFile(string path, TimeSpan? dedupTtl = null)
        {
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var readers = JsonSerializer.Deserialize<List<Reader>>(json, options) ?? new List<Reader>();

            var mgr = new ReaderManager(dedupTtl);
            foreach (var r in readers) mgr.AddReader(r.Id, r.Connection);
            return mgr;
        }

        public void AddReader(string id, ReaderConnectionOptions options)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ReaderManager));
            _adapters.TryAdd(id, null!);
        }

        public async Task StartAllAsync(Func<string, ReaderConnectionOptions> connectionResolver)
        {
            await _startStopLock.WaitAsync();
            try
            {
                var tasks = new List<Task>();
                foreach (var id in _adapters.Keys) tasks.Add(StartOneAsync(id, connectionResolver(id)));
                await Task.WhenAll(tasks);
            }
            finally { _startStopLock.Release(); }
        }

        private async Task StartOneAsync(string id, ReaderConnectionOptions options)
        {
            await RetryPolicy.ExecuteWithBackoffAsync(async () =>
            {
                var adapter = await ReaderFactory.AutoDetectAsync(options);
                adapter.TagRead += AdapterOnTagRead;
                adapter.StatusChanged += AdapterOnStatusChanged;
                _adapters[id] = adapter;
            });
        }

        private void AdapterOnStatusChanged(object? sender, ReaderStatusEventArgs e) => ReaderStatusChanged?.Invoke(this, e);

        private void AdapterOnTagRead(object? sender, TagReadEventArgs e)
        {
            var key = $"{e.ReaderId}:{e.Tag.Epc}";
            var now = DateTime.UtcNow;

            _dedup.AddOrUpdate(key, now, (_, old) =>
            {
                if ((now - old) > _dedupTtl) return now;
                return old;
            });

            if (_dedup[key] != now) return;

            TagRead?.Invoke(this, e);
        }

        public async Task StopAllAsync()
        {
            await _startStopLock.WaitAsync();
            try
            {
                var tasks = new List<Task>();
                foreach (var kv in _adapters)
                {
                    if (kv.Value != null)
                    {
                        tasks.Add(kv.Value.StopInventoryAsync()
                            .ContinueWith(_ => kv.Value.DisconnectAsync()));
                    }
                }
                await Task.WhenAll(tasks);
            }
            finally { _startStopLock.Release(); }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var kv in _adapters) kv.Value?.Dispose();
            _startStopLock.Dispose();
        }

        public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
    }
}
