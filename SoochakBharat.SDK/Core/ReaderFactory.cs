using System;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading.Tasks;
using SoochakBharat.SDK.Adapters;
using SoochakBharat.SDK.Models.Options;

namespace SoochakBharat.SDK.Core
{
    public static class ReaderFactory
    {
        /// <summary>
        /// Auto-detect readers:
        /// 1) Scan Serial COM ports for SIM3500
        /// 2) Try IP addresses (SIM3500 first, then LLRP)
        /// </summary>
        public static async Task<IReaderAdapter?> AutoDetectAsync(ReaderConnectionOptions baseOptions)
        {
            Debug.WriteLine("ReaderFactory.AutoDetectAsync START");

            // 1. Serial auto-detect
            Debug.WriteLine("Trying SERIAL detection...");
            var serial = await TrySerialAsync(baseOptions);
            if (serial != null)
            {
                Debug.WriteLine("SERIAL reader detected");
                return serial;
            }

            // 2. TCP auto-detect
            Debug.WriteLine("Trying TCP detection...");
            var tcp = await TryTcpAsync(baseOptions);
            if (tcp != null)
            {
                Debug.WriteLine("TCP reader detected");
                return tcp;
            }

            Debug.WriteLine("AutoDetect FAILED: no reader found");
            return null;
        }

        // ---------- SERIAL DETECTION ----------
        private static async Task<IReaderAdapter?> TrySerialAsync(ReaderConnectionOptions baseOptions)
        {
            string[] ports = Array.Empty<string>();
            try
            {
                ports = SerialPort.GetPortNames();
            }
            catch { }

            foreach (var port in ports)
            {
                Debug.WriteLine($"Trying SERIAL port: {port}");

                var opts = CloneOptions(baseOptions);
                opts.Address = port;
                opts.Transport = ReaderTransportType.Serial;
                opts.BaudRate = 115200;

                var adapter = new Sim3500Adapter();

                try
                {
                    bool ok = await adapter.ConnectAsync(opts);
                    Debug.WriteLine($"SERIAL {port} result = {ok}");

                    if (ok)
                        return adapter;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SERIAL {port} exception: {ex.Message}");
                }
            }

            return null;
        }

        // ---------- TCP/IP DETECTION ----------
        private static async Task<IReaderAdapter?> TryTcpAsync(ReaderConnectionOptions baseOptions)
        {
            var candidates = new System.Collections.Generic.List<string>();

            // user-provided first
            if (!string.IsNullOrWhiteSpace(baseOptions.Address))
                candidates.Add(baseOptions.Address);

            if (!string.IsNullOrWhiteSpace(baseOptions.IpAddress))
                candidates.Add(baseOptions.IpAddress);

            // common OEM defaults
            candidates.AddRange(new[]
            {
                "192.168.1.100",
                "192.168.0.100",
                "10.10.10.100"
            });

            foreach (var ip in candidates)
            {
                Debug.WriteLine($"Trying TCP IP: {ip}");

                var opts = CloneOptions(baseOptions);
                opts.Transport = ReaderTransportType.TcpIp;
                opts.Address = ip;
                opts.Port = opts.Port ?? 5084;

                var sim = new Sim3500Adapter();
                try
                {
                    bool ok = await sim.ConnectAsync(opts);
                    Debug.WriteLine($"TCP {ip} SIM3500 result = {ok}");

                    if (ok)
                        return sim;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"TCP {ip} SIM3500 exception: {ex.Message}");
                }
            }

            return null;
        }

        // ---------- CLONE ----------
        private static ReaderConnectionOptions CloneOptions(ReaderConnectionOptions src)
        {
            return new ReaderConnectionOptions
            {
                Address = src.Address,
                Port = src.Port,
                AntPorts = src.AntPorts,
                Transport = src.Transport,
                ProtocolHint = src.ProtocolHint,
                Username = src.Username,
                Password = src.Password,
                BaudRate = src.BaudRate,
                IpAddress = src.IpAddress
            };
        }
    }
}
