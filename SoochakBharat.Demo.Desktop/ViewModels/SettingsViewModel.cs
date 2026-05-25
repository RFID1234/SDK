#nullable enable
using System;
using System.Windows.Input;
using SoochakBharat.Demo.Desktop.Services;

namespace SoochakBharat.Demo.Desktop.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ReaderSession _session;
        private string _region = "NA";
        private string _protocol = "GEN2";
        private string _sessionValue = "1";

        public string Region
        {
            get => _region;
            set => SetProperty(ref _region, value);
        }

        public string Protocol
        {
            get => _protocol;
            set => SetProperty(ref _protocol, value);
        }

        public string SessionValue
        {
            get => _sessionValue;
            set => SetProperty(ref _sessionValue, value);
        }

        public bool SupportsSession => false; // placeholder until adapters expose it

        public string CapabilitiesText
        {
            get
            {
                var caps = _session.Capabilities;
                if (caps == null)
                    return "No reader connected. Connect a reader to view capabilities.";

                return $"Model: {caps.Model ?? "N/A"}\n" +
                       $"Vendor: {caps.VendorName ?? "N/A"}\n" +
                       $"Firmware: {caps.FirmwareVersion ?? "N/A"}\n" +
                       $"Hardware: {caps.HardwareVersion ?? "N/A"}\n" +
                       $"Max Antennas: {caps.MaxAntennas}\n" +
                       $"GPI Support: {(caps.SupportsGpi ? "Yes" : "No")}\n" +
                       $"GPO Support: {(caps.SupportsGpo ? "Yes" : "No")}\n" +
                       $"Fast Read: {(caps.SupportsFastRead ? "Yes" : "No")}";
            }
        }

        public ICommand SaveCommand { get; }

        public SettingsViewModel(ReaderSession session)
        {
            _session = session;
            _session.StatusChanged += (_, __) => OnPropertyChanged(nameof(CapabilitiesText));
            SaveCommand = new RelayCommand(_ => Persist());
        }

        private void Persist()
        {
            _session.Logs.Add(new Models.LogEvent
            {
                Level = Models.LogLevel.Info,
                Message = $"Settings updated: Region={Region}, Protocol={Protocol}, Session={SessionValue}",
                Source = "Settings"
            });
        }
    }
}

