using SoochakBharat.SDK.Models.Options;

namespace SoochakBharat.SDK.Models
{
    public class Reader
    {
        public string Id { get; set; } = string.Empty;
        public ReaderConnectionOptions Connection { get; set; } = new();
        public InventoryOptions Inventory { get; set; } = new();
        public ReaderCapabilities Capabilities { get; set; } = new();
    }
}
