using System;
using System.Threading;
using System.Threading.Tasks;
using SoochakBharat.SDK.Events;
using SoochakBharat.SDK.Models;
using SoochakBharat.SDK.Models.Options;
using SoochakBharat.SDK.Transport;

namespace SoochakBharat.SDK.Protocols.Llrp
{
    public class LlrpSession
    {
        private readonly ITransport _transport;
        private readonly LlrpParser _parser = new();
        private readonly LlrpEncoder _encoder = new();
        private CancellationTokenSource? _cts;

        public event EventHandler<TagData>? TagReported;
        public event EventHandler<ReaderStatusEventArgs>? StatusChanged;

        public LlrpSession(ITransport transport) { _transport = transport; }

        public async Task InitializeAsync()
        {
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => ReadLoopAsync(_cts.Token));
        }

        public async Task StartInventoryAsync(InventoryOptions options)
        {
            await Task.CompletedTask;
        }

        public async Task StopInventoryAsync() { await Task.CompletedTask; }

        public async Task ShutdownAsync() { _cts?.Cancel(); await Task.CompletedTask; }

        private async Task ReadLoopAsync(CancellationToken token)
        {
            var header = new byte[10];
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int read = await _transport.ReadAsync(header, 0, header.Length, token);
                    if (read <= 0) { await Task.Delay(50, token); continue; }
                    int length = _parser.GetMessageLength(header, read);
                    if (length <= 0) continue;

                    var buf = new byte[length];
                    Buffer.BlockCopy(header, 0, buf, 0, read);
                    int remain = length - read;
                    int offset = read;
                    while (remain > 0)
                    {
                        int r = await _transport.ReadAsync(buf, offset, remain, token);
                        if (r <= 0) throw new Exception("LLRP stream closed");
                        offset += r; remain -= r;
                    }

                    if (_parser.IsTagReport(buf))
                    {
                        // TODO: Properly convert parsed LLRP item → TagData and raise event.
                        // For now, LLRP functionality is disabled until implementation stage.

                        // Example placeholder (disabled):
                        // var tag = new TagData { EPC = item.Epc, RSSI = item.Rssi };
                        // TagReported?.Invoke(this, tag);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { StatusChanged?.Invoke(this, new ReaderStatusEventArgs("LLRP", ReaderConnectionState.Error, ex.Message)); }
        }
    }
}
