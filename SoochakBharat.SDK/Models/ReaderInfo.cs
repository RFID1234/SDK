namespace SoochakBharat.SDK.Models
{
    /// <summary>
    /// Lightweight description of a connected reader instance.
    /// </summary>
    public class ReaderInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? Firmware { get; set; }
        public string? Region { get; set; }
        public string? Vendor { get; set; }
        public bool IsConnected { get; set; }
        public ReaderCapabilities? Capabilities { get; set; }
    }
}


