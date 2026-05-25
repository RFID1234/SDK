namespace SoochakBharat.SDK.Models.Options
{
    public enum ReaderTransportType { Auto, TcpIp, Serial }
    public enum ReaderProtocolHint { Auto, Llrp, Sim3500 }

    public class ReaderConnectionOptions
    {
        public string Address { get; set; } = string.Empty;
        public int? Port { get; set; }
        public int? AntPorts { get; set; }
        public ReaderTransportType Transport { get; set; } = ReaderTransportType.Auto;
        public ReaderProtocolHint ProtocolHint { get; set; } = ReaderProtocolHint.Auto;
        public string? Username { get; set; }
        public string? Password { get; set; }

        public int BaudRate { get; set; } = 115200;
        public string? IpAddress { get; set; }

    }
}
