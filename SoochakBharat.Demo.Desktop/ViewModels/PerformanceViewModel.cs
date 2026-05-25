#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using SoochakBharat.Demo.Desktop.Services;
using SoochakBharat.SDK.Events;

namespace SoochakBharat.Demo.Desktop.ViewModels
{
    public class PerformanceViewModel : BaseViewModel
    {
        private readonly ReaderSession _session;
        private readonly List<(DateTime ts, int rssi)> _window = new();
        private readonly DispatcherTimer _timer;
        private DateTime? _inventoryStarted;
        private double _tagsPerSecond;
        private double _avgRssi;
        private int _activeAntennas;
        private string _uptime = "00:00:00";

        public double TagsPerSecond
        {
            get => _tagsPerSecond;
            private set => SetProperty(ref _tagsPerSecond, value);
        }

        public double AvgRssi
        {
            get => _avgRssi;
            private set => SetProperty(ref _avgRssi, value);
        }

        public int ActiveAntennas
        {
            get => _activeAntennas;
            private set => SetProperty(ref _activeAntennas, value);
        }

        public string Uptime
        {
            get => _uptime;
            private set => SetProperty(ref _uptime, value);
        }

        public PerformanceViewModel(ReaderSession session)
        {
            _session = session;
            _session.TagRead += OnTagRead;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (_, __) => Refresh();
            _timer.Start();
            ActiveAntennas = _session.GetActiveAntennas().Count;
        }

        public void MarkInventoryStarted()
        {
            _inventoryStarted = DateTime.Now;
        }

        public void MarkInventoryStopped()
        {
            _inventoryStarted = null;
            _window.Clear();
            TagsPerSecond = 0;
            AvgRssi = 0;
            Uptime = "00:00:00";
        }

        private void OnTagRead(object? sender, TagReadEventArgs e)
        {
            _window.Add((DateTime.Now, e.Tag.Rssi));
            if (_inventoryStarted == null)
                _inventoryStarted = DateTime.Now;

            ActiveAntennas = _session.GetActiveAntennas().Count;
            Refresh();
        }

        private void Refresh()
        {
            var cutoff = DateTime.Now.AddSeconds(-10);
            _window.RemoveAll(w => w.ts < cutoff);

            var seconds = Math.Max(1, (DateTime.Now - cutoff).TotalSeconds);
            TagsPerSecond = Math.Round(_window.Count / seconds, 2);
            AvgRssi = _window.Count == 0 ? 0 : Math.Round(_window.Average(w => w.rssi), 1);

            if (_inventoryStarted != null)
            {
                var span = DateTime.Now - _inventoryStarted.Value;
                Uptime = span.ToString(@"hh\:mm\:ss");
            }
        }
    }
}

