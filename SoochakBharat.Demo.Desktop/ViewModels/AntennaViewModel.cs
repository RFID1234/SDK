#nullable enable
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SoochakBharat.Demo.Desktop.Services;

namespace SoochakBharat.Demo.Desktop.ViewModels
{
    public class AntennaPortModel : BaseViewModel
    {
        private bool _isEnabled;

        public int Index { get; set; }
        public bool IsSupported { get; set; } = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
    }

    public class AntennaViewModel : BaseViewModel
    {
        private readonly ReaderSession _session;
        public ObservableCollection<AntennaPortModel> Ports { get; } = new();

        public ICommand ToggleCommand { get; }

        public bool SupportsAntennaConfig => _session.Capabilities.MaxAntennas > 0;

        public AntennaViewModel(ReaderSession session)
        {
            _session = session;
            ToggleCommand = new RelayCommand(p =>
            {
                if (p is AntennaPortModel port)
                {
                    port.IsEnabled = !port.IsEnabled;
                    PushToSession();
                }
            });

            RefreshFromCapabilities();
        }

        public void RefreshFromCapabilities()
        {
            Ports.Clear();
            var count = _session.Capabilities.MaxAntennas > 0 ? _session.Capabilities.MaxAntennas : 4;
            var active = _session.GetActiveAntennas();
            for (int i = 1; i <= count; i++)
            {
                Ports.Add(new AntennaPortModel
                {
                    Index = i,
                    IsEnabled = active.Contains(i),
                    IsSupported = true
                });
            }
        }

        private void PushToSession()
        {
            var active = Ports.Where(p => p.IsEnabled).Select(p => p.Index).ToArray();
            _session.SetActiveAntennas(active);
        }
    }
}

