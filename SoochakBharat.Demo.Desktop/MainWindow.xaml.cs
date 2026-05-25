using System.Windows;
using SoochakBharat.Demo.Desktop.ViewModels;

namespace SoochakBharat.Demo.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}