namespace SoochakBharat.SDK.Models
{
    public class ReaderCapabilities
    {
        public int MaxAntennas { get; set; }
        public bool SupportsGpi { get; set; }
        public bool SupportsGpo { get; set; }
        public bool SupportsFastRead { get; set; }
        public string? Model { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? HardwareVersion { get; set; }
        public string? VendorName { get; set; }
    }
}
