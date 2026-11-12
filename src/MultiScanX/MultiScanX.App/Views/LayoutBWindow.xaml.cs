using System.Windows;
using System.Windows.Controls;
using MultiScanX.App.ViewModels;

namespace MultiScanX.App.Views
{
    public partial class LayoutBWindow : Window
    {
        public LayoutBWindow()
        {
            InitializeComponent();
        }

        private void BtnNav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string idxStr && int.TryParse(idxStr, out int idx))
            {
                RightDockTabs.SelectedIndex = idx;
            }
        }

        private void BtnStep1Prepare_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm) vm.ExecuteStep1PrepareForLoading();
        }

        private void BtnStep2Vacuum_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm) vm.ExecuteStep2ToggleVacuum();
        }

        private async void BtnStep3Start_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm) await vm.StartScanAsync();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm) vm.StopScan();
        }

        private void BtnResQhd_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            Width = 2560;
            Height = 1440;
            Left = 0;
            Top = 0;
        }

        private void BtnResFhd_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            Width = 1920;
            Height = 1080;
            Left = 20;
            Top = 20;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void BtnOpenLayoutA_Click(object sender, RoutedEventArgs e)
        {
            var win = new LayoutAWindow { DataContext = this.DataContext };
            win.Show();
        }

        private void BtnOpenLayoutC_Click(object sender, RoutedEventArgs e)
        {
            var win = new LayoutCWindow { DataContext = this.DataContext };
            win.Show();
        }
    }
}

