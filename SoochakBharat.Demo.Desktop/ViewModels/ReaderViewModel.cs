#nullable enable
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SoochakBharat.Demo.Desktop.Models;
using SoochakBharat.Demo.Desktop.Services;
using SoochakBharat.SDK.Events;
using SoochakBharat.SDK.Models.Options;

namespace SoochakBharat.Demo.Desktop.ViewModels
{
    public class ReaderViewModel : BaseViewModel
    {
        private readonly ReaderSession _session;
        private string _address = "192.168.1.100";
        private int _antennaPorts = 4;
        private string _status = "Disconnected";
        private bool _isBusy;

        public ReaderViewModel(ReaderSession session)
        {
            _session = session;
            _session.StatusChanged += (_, e) => Status = e.State.ToString();

            ConnectCommand = new RelayCommand(async _ => await ConnectAsync(), _ => !_isBusy);
            DisconnectCommand = new RelayCommand(async _ => await DisconnectAsync(), _ => !_isBusy && _session.IsConnected);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public int AntennaPorts
        {
            get => _antennaPorts;
            set => SetProperty(ref _antennaPorts, value);
        }

        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        public ReaderIdentity Identity => _session.Identity;

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }

        private async Task ConnectAsync()
        {
            _isBusy = true;
            try
            {
                var options = new ReaderConnectionOptions
                {
                    Address = Address,
                    AntPorts = AntennaPorts
                };

                bool ok = await _session.ConnectAsync(options, adapterKey: "sim3500");
                if (!ok)
                    MessageBox.Show("Failed to connect reader.", "Connection", MessageBoxButton.OK, MessageBoxImage.Error);

                OnPropertyChanged(nameof(Identity));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isBusy = false;
                (ConnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DisconnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private async Task DisconnectAsync()
        {
            _isBusy = true;
            try
            {
                await _session.DisconnectAsync();
            }
            finally
            {
                _isBusy = false;
                (ConnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DisconnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }
}

