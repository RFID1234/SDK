#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SoochakBharat.Demo.Desktop.Models;
using SoochakBharat.Demo.Desktop.Services;
using SoochakBharat.SDK.Events;

namespace SoochakBharat.Demo.Desktop.ViewModels
{
    public class InventoryViewModel : BaseViewModel
    {
        private readonly ReaderSession _session;
        private readonly Dictionary<string, TagDisplay> _tagCache = new();
        private bool _isRunning;
        private bool _dedupEnabled = true;
        private readonly PerformanceViewModel? _performance;

        public ObservableCollection<TagDisplay> Tags { get; } = new();

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ClearCommand { get; }

        public bool IsRunning
        {
            get => _isRunning;
            private set => SetProperty(ref _isRunning, value);
        }

        public bool Deduplicate
        {
            get => _dedupEnabled;
            set => SetProperty(ref _dedupEnabled, value);
        }

        public InventoryViewModel(ReaderSession session, PerformanceViewModel? performance = null)
        {
            _session = session;
            _performance = performance;
            _session.TagRead += OnTagRead;

            StartCommand = new RelayCommand(async _ => await StartInventory(), _ => !_isRunning);
            StopCommand = new RelayCommand(async _ => await StopInventory(), _ => _isRunning);
            ClearCommand = new RelayCommand(_ => Clear());
        }

        private async System.Threading.Tasks.Task StartInventory()
        {
            if (!_session.IsConnected)
            {
                MessageBox.Show("Connect a reader first.", "Inventory", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Clear();
            IsRunning = await _session.StartInventoryAsync();
            _performance?.MarkInventoryStarted();
            RaiseCommandStates();
        }

        private async System.Threading.Tasks.Task StopInventory()
        {
            await _session.StopInventoryAsync();
            IsRunning = false;
            _performance?.MarkInventoryStopped();
            RaiseCommandStates();
        }

        private void Clear()
        {
            _tagCache.Clear();
            Tags.Clear();
        }

        private void OnTagRead(object? sender, TagReadEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (Deduplicate && _tagCache.TryGetValue(e.Tag.Epc, out var existing))
                    {
                        existing.ReadCount += e.Tag.ReadCount;
                        existing.Timestamp = DateTime.Now;
                        OnPropertyChanged(nameof(Tags));
                        return;
                    }

                    var display = new TagDisplay
                    {
                        Epc = e.Tag.Epc,
                        Antenna = e.Tag.Antenna,
                        Rssi = e.Tag.Rssi,
                        ReadCount = e.Tag.ReadCount,
                        FrequencyKHz = e.Tag.FrequencyKHz,
                        Timestamp = DateTime.Now
                    };

                    _tagCache[e.Tag.Epc] = display;
                    Tags.Insert(0, display);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Inventory UI error: {ex}");
                }
            });
        }

        private void RaiseCommandStates()
        {
            (StartCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (StopCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}

