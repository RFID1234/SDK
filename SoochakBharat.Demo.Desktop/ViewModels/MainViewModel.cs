#nullable enable
using System.Collections.ObjectModel;
using SoochakBharat.Demo.Desktop.Models;
using SoochakBharat.Demo.Desktop.Services;

namespace SoochakBharat.Demo.Desktop.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ReaderSession Session { get; }
        public ReaderViewModel ReaderView { get; }
        public PerformanceViewModel PerformanceView { get; }
        public InventoryViewModel InventoryView { get; }
        public AntennaViewModel AntennaView { get; }
        public EventsViewModel EventsView { get; }
        public SettingsViewModel SettingsView { get; }

        public ObservableCollection<NavItem> Navigation { get; }

        private NavItem? _selectedNav;
        private object? _currentViewModel;

        public NavItem? SelectedNav
        {
            get => _selectedNav;
            set
            {
                if (SetProperty(ref _selectedNav, value))
                    CurrentViewModel = value?.ViewModel;
            }
        }

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public MainViewModel()
        {
            Session = new ReaderSession();
            PerformanceView = new PerformanceViewModel(Session);
            ReaderView = new ReaderViewModel(Session);
            InventoryView = new InventoryViewModel(Session, PerformanceView);
            AntennaView = new AntennaViewModel(Session);
            EventsView = new EventsViewModel(Session);
            SettingsView = new SettingsViewModel(Session);

            Session.StatusChanged += (_, __) => AntennaView.RefreshFromCapabilities();

            Navigation = new ObservableCollection<NavItem>
            {
                new NavItem { Title = "Readers", Icon = "\uE8FA", ViewModel = ReaderView },
                new NavItem { Title = "Inventory", Icon = "\uE73E", ViewModel = InventoryView },
                new NavItem { Title = "Antennas", Icon = "\uE7B5", ViewModel = AntennaView },
                new NavItem { Title = "Performance", Icon = "\uE9D9", ViewModel = PerformanceView },
                new NavItem { Title = "Events & Logs", Icon = "\uE7BA", ViewModel = EventsView },
                new NavItem { Title = "Settings", Icon = "\uE713", ViewModel = SettingsView }
            };

            SelectedNav = Navigation[0];
        }
    }
}

