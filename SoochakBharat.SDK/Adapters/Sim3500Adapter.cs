using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModuleTech;
using SoochakBharat.SDK.Models;
using SoochakBharat.SDK.Models.Options;
using SoochakBharat.SDK.Events;

// Alias to avoid Reader ambiguity
using OEMReader = ModuleTech.Reader;

namespace SoochakBharat.SDK.Adapters
{
    // ------------------------------
    // Simple file logger
    // ------------------------------
    static class Sim3500Log
    {
        private static readonly object _lock = new();
        private static readonly string _path = "sim3500.log";

        public static void Write(string message)
        {
            lock (_lock)
            {
                File.AppendAllText(
                    _path,
                    $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n"
                );
            }
        }
    }

    // ------------------------------
    // SIM3500 Adapter (PHASE 1)
    // ------------------------------
    public class Sim3500Adapter : IReaderAdapter
    {
        private OEMReader? _reader;
        private int _antPortCount;

        public event EventHandler<TagReadEventArgs>? TagRead;
        public event EventHandler<ReaderStatusEventArgs>? StatusChanged;

        // ------------------------------
        // CONNECT
        // ------------------------------
        public async Task<bool> ConnectAsync(ReaderConnectionOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Address))
                throw new ArgumentException("Reader address is required");

            if (options.AntPorts is null || options.AntPorts <= 0)
                throw new ArgumentException("Antenna port count is required");

            try
            {
                _antPortCount = options.AntPorts.Value;

                Sim3500Log.Write(
                    $"Connecting to {options.Address}, AntPorts={_antPortCount}"
                );

                _reader = OEMReader.Create(
                    options.Address,
                    Region.NA,
                    _antPortCount
                );

                _reader.TagsRead += OnTagsRead;

                StatusChanged?.Invoke(
                    this,
                    new ReaderStatusEventArgs(
                        readerId: options.Address,
                        state: ReaderConnectionState.Connected,
                        message: "SIM3500 connected"
                    )
                );

                Sim3500Log.Write("Reader.Create() successful");
                return true;
            }
            catch (Exception ex)
            {
                Sim3500Log.Write("Connect FAILED: " + ex);
                return false;
            }
        }

        // ------------------------------
        // DISCONNECT
        // ------------------------------
        public async Task<bool> DisconnectAsync()
        {
            try
            {
                if (_reader != null)
                {
                    _reader.TagsRead -= OnTagsRead;
                    _reader.Disconnect();
                    _reader = null;
                }

                StatusChanged?.Invoke(
                    this,
                    new ReaderStatusEventArgs(
                        readerId: "sim3500",
                        state: ReaderConnectionState.Disconnected,
                        message: "SIM3500 disconnected"
                    )
                );

                Sim3500Log.Write("Reader disconnected");
                return true;
            }
            catch (Exception ex)
            {
                Sim3500Log.Write("Disconnect FAILED: " + ex);
                return false;
            }
        }

        // ------------------------------
        // START INVENTORY (OEM-CORRECT)
        // ------------------------------
        public async Task<bool> StartInventoryAsync(InventoryOptions? options = null)
        {
            if (_reader == null)
            {
                Sim3500Log.Write("StartInventory called but reader is NULL");
                return false;
            }

            try
            {
                Sim3500Log.Write("Configuring inventory");

                // -------- 1. Antennas (OEM style) --------
                int[] antennas = Enumerable.Range(1, _antPortCount).ToArray();

                // -------- 2. BackReadOption --------
                var bro = new BackReadOption
                {
                    IsFastRead = false,
                    ReadDuration = 0,
                    ReadInterval = 0
                };

                bro.FRTMetadata = new BackReadOption.FastReadTagMetaData
                {
                    IsAntennaID = true,
                    IsFrequency = true,
                    IsRSSI = true,
                    IsReadCnt = true,
                    IsTimestamp = true,
                    IsEmdData = true
                };

                _reader.ParamSet("BackReadOption", bro);

                // -------- 3. ReadPlan (FIXES ERROR 9002) --------
                var readPlan = new SimpleReadPlan(
                    TagProtocol.GEN2,
                    antennas,
                    30
                );

                _reader.ParamSet("ReadPlan", readPlan);

                // -------- 4. Start --------
                Sim3500Log.Write("Calling StartReading()");
                _reader.StartReading();

                Sim3500Log.Write("Inventory started successfully");
                return true;
            }
            catch (Exception ex)
            {
                Sim3500Log.Write("StartInventory FAILED: " + ex);
                return false;
            }
        }

        // ------------------------------
        // STOP INVENTORY
        // ------------------------------
        public async Task<bool> StopInventoryAsync()
        {
            if (_reader == null)
                return false;

            try
            {
                _reader.StopReading();
                Sim3500Log.Write("StopReading() called");
                return true;
            }
            catch (Exception ex)
            {
                Sim3500Log.Write("StopReading FAILED: " + ex);
                return false;
            }
        }

        // ------------------------------
        // TAG CALLBACK
        // ------------------------------
        private void OnTagsRead(object sender, OEMReader.TagsReadEventArgs e)
        {
            int count = e.Tags?.Count() ?? 0;
            Sim3500Log.Write($"TagsRead event fired. Count={count}");

            if (count == 0)
                return;

            foreach (var t in e.Tags)
            {
                Sim3500Log.Write(
                    $"TAG EPC={t.EPCString}, Ant={t.Antenna}, RSSI={t.Rssi}, Reads={t.ReadCount}"
                );

                var tag = SoochakBharat.SDK.Models.TagData.FromSim3500(t);

                TagRead?.Invoke(
                    this,
                    new TagReadEventArgs(tag, "sim3500")
                );
            }
        }

        // ------------------------------
        // DISPOSE
        // ------------------------------
        public void Dispose()
        {
            try { _reader?.Disconnect(); } catch { }
            _reader = null;
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
