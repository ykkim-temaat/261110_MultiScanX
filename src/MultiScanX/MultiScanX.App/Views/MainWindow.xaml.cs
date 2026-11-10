using System.Windows;
using MultiScanX.App.ViewModels;

namespace MultiScanX.App.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.StartScanAsync();
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.StopScan();
            }
        }
    }
}

