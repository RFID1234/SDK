using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SoochakBharat.SDK.Api
{
    /// <summary>
    /// Minimal HttpListener-based local API surface for querying readers and tags.
    /// Not intended for Internet exposure; bind to localhost only.
    /// </summary>
    public class LocalApiServer : IDisposable
    {
        private readonly HttpListener _listener = new();
        private readonly IReaderManager _manager;
        private CancellationTokenSource? _cts;

        public bool IsRunning => _cts != null && !_cts.IsCancellationRequested;

        public LocalApiServer(IReaderManager manager, int port = 50850)
        {
            _manager = manager;
            _listener.Prefixes.Add($"http://localhost:{port}/");
        }

        public void Start()
        {
            if (IsRunning)
                return;

            _cts = new CancellationTokenSource();
            _listener.Start();
            Task.Run(() => AcceptLoopAsync(_cts.Token));
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            _cts!.Cancel();
            _listener.Stop();
        }

        private async Task AcceptLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                HttpListenerContext? ctx = null;
                try
                {
                    ctx = await _listener.GetContextAsync();
                }
                catch when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch
                {
                    continue;
                }

                _ = Task.Run(() => HandleRequestAsync(ctx), ct);
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext ctx)
        {
            try
            {
                var request = ctx.Request;
                var response = ctx.Response;
                response.ContentType = "application/json";

                var path = request.Url?.AbsolutePath?.TrimEnd('/') ?? "/";

                if (request.HttpMethod == "GET" && path.Equals("/readers", StringComparison.OrdinalIgnoreCase))
                {
                    await WriteJsonAsync(response, _manager.ListReaders());
                }
                else if (request.HttpMethod == "GET" && path.StartsWith("/readers/", StringComparison.OrdinalIgnoreCase))
                {
                    var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length == 3 && segments[2].Equals("status", StringComparison.OrdinalIgnoreCase))
                    {
                        var id = segments[1];
                        await WriteJsonAsync(response, _manager.GetStats(id));
                    }
                    else if (segments.Length == 3 && segments[2].Equals("tags", StringComparison.OrdinalIgnoreCase))
                    {
                        var id = segments[1];
                        await WriteJsonAsync(response, _manager.GetTags(id));
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch
            {
                try { ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError; } catch { }
            }
            finally
            {
                try { ctx.Response.OutputStream.Close(); } catch { }
            }
        }

        private static async Task WriteJsonAsync(HttpListenerResponse response, object payload)
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            var bytes = Encoding.UTF8.GetBytes(json);
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }

        public void Dispose()
        {
            Stop();
            _listener.Close();
        }
    }
}


