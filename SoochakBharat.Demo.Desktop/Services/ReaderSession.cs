#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SoochakBharat.Demo.Desktop.Models;
using SoochakBharat.SDK.Adapters;
using SoochakBharat.SDK.Events;
using SoochakBharat.SDK.Models;
using SoochakBharat.SDK.Models.Options;

namespace SoochakBharat.Demo.Desktop.Services
{
    /// <summary>
    /// Lightweight session wrapper around the SDK adapters.
    /// Keeps UI concerns (identity, logs, active antennas) separate from low-level logic.
    /// </summary>
    public class ReaderSession : IDisposable
    {
        private readonly object _sync = new();
        private IReaderAdapter? _adapter;
        private readonly HashSet<int> _activeAntennas = new();

        public ReaderConnectionState ConnectionState { get; private set; } = ReaderConnectionState.Disconnected;
        public ReaderIdentity Identity { get; } = new();
        public ReaderCapabilities Capabilities { get; private set; } = new();
        public ObservableCollection<LogEvent> Logs { get; } = new();

        public event EventHandler<TagReadEventArgs>? TagRead;
        public event EventHandler<ReaderStatusEventArgs>? StatusChanged;

        public bool IsConnected => ConnectionState == ReaderConnectionState.Connected;

        public async Task<bool> ConnectAsync(ReaderConnectionOptions options, string adapterKey = "sim3500")
        {
            await DisconnectInternal();

            _adapter = adapterKey switch
            {
                // Future: plug-in additional adapters (LLRP, etc.)
                _ => new Sim3500Adapter()
            };

            _adapter.TagRead += OnTagRead;
            _adapter.StatusChanged += OnStatusChanged;

            ConnectionState = ReaderConnectionState.Connecting;
            PublishLog(LogLevel.Info, $"Connecting to {options.Address}...");

            var ok = await _adapter.ConnectAsync(options);
            if (!ok)
            {
                PublishLog(LogLevel.Error, "Connection failed.");
                ConnectionState = ReaderConnectionState.Error;
                return false;
            }

            ConnectionState = ReaderConnectionState.Connected;
            Identity.Address = options.Address ?? string.Empty;
            Identity.Model = "SIM3500";
            Identity.Firmware = Identity.Firmware ?? "N/A";
            Identity.Region = "NA";
            Identity.Status = "Connected";

            Capabilities = new ReaderCapabilities
            {
                MaxAntennas = options.AntPorts ?? 4,
                Model = Identity.Model,
                FirmwareVersion = Identity.Firmware,
                VendorName = "SoochakBharat"
            };

            _activeAntennas.Clear();
            for (int i = 1; i <= Capabilities.MaxAntennas; i++)
            {
                _activeAntennas.Add(i);
            }

            PublishLog(LogLevel.Info, "Reader connected.");
            return true;
        }

        public async Task<bool> DisconnectAsync()
        {
            var result = await DisconnectInternal();
            PublishLog(LogLevel.Info, "Reader disconnected.");
            return result;
        }

        public async Task<bool> StartInventoryAsync(InventoryOptions? options = null)
        {
            if (_adapter == null) return false;
            var ok = await _adapter.StartInventoryAsync(options);
            if (ok)
            {
                PublishLog(LogLevel.Info, "Inventory started.");
            }
            return ok;
        }

        public async Task<bool> StopInventoryAsync()
        {
            if (_adapter == null) return false;
            var ok = await _adapter.StopInventoryAsync();
            if (ok)
            {
                PublishLog(LogLevel.Info, "Inventory stopped.");
            }
            return ok;
        }

        public IReadOnlyCollection<int> GetActiveAntennas() => _activeAntennas.ToList();

        public void SetActiveAntennas(IEnumerable<int> ports)
        {
            lock (_sync)
            {
                _activeAntennas.Clear();
                foreach (var p in ports.Where(p => p > 0 && p <= Capabilities.MaxAntennas))
                    _activeAntennas.Add(p);
            }

            PublishLog(LogLevel.Info, $"Active antennas updated: {string.Join(",", _activeAntennas)}");
        }

        private void OnTagRead(object? sender, TagReadEventArgs e)
        {
            TagRead?.Invoke(this, e);
        }

        private void OnStatusChanged(object? sender, ReaderStatusEventArgs e)
        {
            ConnectionState = e.State;
            Identity.Status = e.State.ToString();
            StatusChanged?.Invoke(this, e);
            PublishLog(LogLevel.Info, $"Status: {e.State} ({e.Message})");
        }

        private async Task<bool> DisconnectInternal()
        {
            if (_adapter == null)
                return true;

            try
            {
                _adapter.TagRead -= OnTagRead;
                _adapter.StatusChanged -= OnStatusChanged;

                var ok = await _adapter.DisconnectAsync();
                await _adapter.DisposeAsync();
                _adapter = null;
                ConnectionState = ReaderConnectionState.Disconnected;
                Identity.Status = "Disconnected";
                return ok;
            }
            catch
            {
                ConnectionState = ReaderConnectionState.Error;
                return false;
            }
        }

        private void PublishLog(LogLevel level, string message)
        {
            Logs.Add(new LogEvent
            {
                Level = level,
                Message = message,
                Timestamp = DateTime.Now,
                Source = "Reader"
            });
        }

        public void Dispose()
        {
            DisconnectInternal().GetAwaiter().GetResult();
        }
    }
}

