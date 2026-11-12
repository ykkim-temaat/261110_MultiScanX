using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MultiScanX.App.ViewModels;

namespace MultiScanX.App.Views.Panels
{
    public partial class IoHardwareCheckControl : UserControl
    {
        public IoHardwareCheckControl()
        {
            InitializeComponent();
        }

        private void BtnStepXY_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string valStr &&
                double.TryParse(valStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double step) &&
                DataContext is MainViewModel vm)
            {
                vm.JogStepXY = step;
            }
        }

        private void BtnStepZ_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string valStr &&
                double.TryParse(valStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double step) &&
                DataContext is MainViewModel vm)
            {
                vm.JogStepZ = step;
            }
        }

        private void BtnJog_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string cmd && DataContext is MainViewModel vm)
            {
                var parts = cmd.Split(':');
                if (parts.Length == 2 && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double dir))
                {
                    vm.JogAxis(parts[0], dir);
                }
            }
        }

        private void BtnHomeAll_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.HomeAllAxes();
            }
        }

        private void BtnToggleBrake_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.IsZBrakeReleased = !vm.IsZBrakeReleased;
            }
        }

        private void BtnLoadCellZero_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ZeroLoadCell();
            }
        }

        private void BtnLoadCellTare_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.TareLoadCell();
            }
        }

        private void BtnLoadCellApplySp_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ApplyLoadCellSetPoints();
            }
        }

        private void BtnToggleIoBit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IoBitViewModel bit)
            {
                bit.IsOn = !bit.IsOn;
            }
        }

        private void BtnSaveIoConfig_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SaveCurrentConfigAndRecipe();
            }
        }
    }
}

