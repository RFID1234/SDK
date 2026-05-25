using System;
namespace SoochakBharat.SDK.Core.Licensing
{
    public static class LicenseManager
    {
        public static bool IsInternalUse { get; set; } = true;
        public static void EnsureLicensed(string feature) { }
    }
}
