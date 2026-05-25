#nullable enable
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using SoochakBharat.Demo.Desktop.Models;
using SoochakBharat.Demo.Desktop.Services;

namespace SoochakBharat.Demo.Desktop.ViewModels
{
    public class EventsViewModel : BaseViewModel
    {
        private readonly ReaderSession _session;
        private readonly ObservableCollection<LogEvent> _allEvents = new();
        private LogLevel _selectedLevel = LogLevel.Info;
        private readonly DispatcherTimer _logFileTimer;
        private long _lastLogFilePosition = 0;
        private const string LogFilePath = "sim3500.log";

        public ObservableCollection<LogEvent> Events => _allEvents;
        public ICollectionView FilteredEvents { get; }

        public ICommand ExportCommand { get; }

        public LogLevel SelectedLevel
        {
            get => _selectedLevel;
            set
            {
                if (SetProperty(ref _selectedLevel, value))
                {
                    FilteredEvents.Refresh();
                }
            }
        }

        public EventsViewModel(ReaderSession session)
        {
            _session = session;
            
            // Merge session logs with file logs
            foreach (var log in _session.Logs)
            {
                _allEvents.Add(log);
            }
            _session.Logs.CollectionChanged += (_, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (LogEvent item in e.NewItems)
                    {
                        _allEvents.Add(item);
                    }
                }
            };

            FilteredEvents = CollectionViewSource.GetDefaultView(_allEvents);
            FilteredEvents.SortDescriptions.Add(new SortDescription(nameof(LogEvent.Timestamp), ListSortDirection.Descending));
            FilteredEvents.Filter = FilterByLevel;

            ExportCommand = new RelayCommand(_ => Export());

            // Poll sim3500.log file every 2 seconds
            _logFileTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _logFileTimer.Tick += OnLogFileTimerTick;
            _logFileTimer.Start();
            
            // Load existing log file entries
            LoadLogFile();
        }

        private void OnLogFileTimerTick(object? sender, EventArgs e)
        {
            LoadLogFile();
        }

        private void LoadLogFile()
        {
            if (!File.Exists(LogFilePath))
                return;

            try
            {
                using var fs = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (fs.Length <= _lastLogFilePosition)
                    return;

                fs.Position = _lastLogFilePosition;
                using var reader = new StreamReader(fs);
                
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var logEvent = ParseLogLine(line);
                    if (logEvent != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _allEvents.Insert(0, logEvent);
                        });
                    }
                }
                
                _lastLogFilePosition = fs.Length;
            }
            catch (Exception ex)
            {
                // Silently handle file read errors
                System.Diagnostics.Debug.WriteLine($"Log file read error: {ex.Message}");
            }
        }

        private LogEvent? ParseLogLine(string line)
        {
            // Parse format: [HH:mm:ss.fff] message
            var match = Regex.Match(line, @"\[(\d{2}:\d{2}:\d{2}\.\d{3})\]\s+(.+)");
            if (!match.Success)
                return null;

            var timestampStr = match.Groups[1].Value;
            var message = match.Groups[2].Value;

            if (!DateTime.TryParseExact(timestampStr, "HH:mm:ss.fff", null, System.Globalization.DateTimeStyles.None, out var timestamp))
            {
                // Use today's date with the time
                timestamp = DateTime.Today.Add(TimeSpan.Parse(timestampStr.Substring(0, 8)));
                if (timestampStr.Length > 8)
                {
                    var ms = int.Parse(timestampStr.Substring(9));
                    timestamp = timestamp.AddMilliseconds(ms);
                }
            }

            var level = LogLevel.Info;
            if (message.Contains("FAILED", StringComparison.OrdinalIgnoreCase) || 
                message.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
                level = LogLevel.Error;
            else if (message.Contains("WARNING", StringComparison.OrdinalIgnoreCase))
                level = LogLevel.Warning;

            return new LogEvent
            {
                Timestamp = timestamp,
                Level = level,
                Message = message,
                Source = "SIM3500"
            };
        }

        private bool FilterByLevel(object obj)
        {
            if (obj is not LogEvent e)
                return false;

            return SelectedLevel switch
            {
                LogLevel.Info => true,
                LogLevel.Warning => e.Level != LogLevel.Info,
                LogLevel.Error => e.Level == LogLevel.Error,
                _ => true
            };
        }

        private void Export()
        {
            try
            {
                var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"rfid-logs-{DateTime.Now:yyyyMMdd-HHmmss}.txt");

                using var writer = new StreamWriter(path);
                foreach (var log in _allEvents.OrderBy(l => l.Timestamp))
                {
                    writer.WriteLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {log.Level}: {log.Message} ({log.Source})");
                }
                
                System.Windows.MessageBox.Show($"Logs exported to:\n{path}", "Export Complete", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Export failed: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}

