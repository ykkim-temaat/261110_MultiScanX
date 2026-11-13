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
            if (sender is Button btn && btn.Tag is IoBitViewModel bit && DataContext is MainViewModel vm)
            {
                // DI 입력 채널은 실물 온라인 모드일 때 AllowDiSimulationOverride가 켜져 있어야만 수동 토글 허용
                if (!bit.IsOutput && vm.IsAxlHardwareOnline && !vm.AllowDiSimulationOverride)
                {
                    return;
                }
                bit.IsOn = !bit.IsOn;
            }
        }

        private void BtnReconnectAxl_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.TryConnectRealAxlHardware(logOnFail: true);
            }
        }

        private async void BtnDoBlinkTest_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.RunDoSequentialBlinkTestAsync();
            }
        }

        private void BtnCnt2ZeroClear_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ZeroClearCnt2Scale();
            }
        }

        private void BtnCnt2ApplyPreset_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ApplyCnt2PresetPosition();
            }
        }

        private void BtnCnt2ApplyEncoder_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ApplyCnt2EncoderConfig();
            }
        }

        private void BtnCnt2ToggleTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ToggleCnt2PositionTrigger();
            }
        }

        private void BtnCnt2OneShot_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.FireCnt2OneShotTrigger();
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
