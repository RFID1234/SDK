#nullable enable
using System;

namespace SoochakBharat.Demo.Desktop.Models
{
    public class NavItem
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = "•";
        public object? ViewModel { get; set; }
        public string Key { get; set; } = Guid.NewGuid().ToString();
    }
}

