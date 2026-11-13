using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MultiScanX.App.ViewModels;

namespace MultiScanX.App.Views.Panels
{
    public partial class IoHardwareCheckControl : UserControl
    {
        private Window? _hostWindow;

        public IoHardwareCheckControl()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                _hostWindow = Window.GetWindow(this);
                if (_hostWindow != null)
                {
                    _hostWindow.Closing -= HostWindow_Closing;
                    _hostWindow.Closing += HostWindow_Closing;
                }
            };
            Unloaded += (_, _) =>
            {
                if (_hostWindow != null)
                {
                    _hostWindow.Closing -= HostWindow_Closing;
                    _hostWindow = null;
                }
            };
        }

        private void HostWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.IsAdminUnlocked && vm.HasIoConfigChanges())
            {
                if (!ConfirmSaveOrRestoreIfDirty(vm))
                {
                    e.Cancel = true;
                }
            }
        }

        private static bool ConfirmSaveOrRestoreIfDirty(MainViewModel vm)
        {
            if (!vm.HasIoConfigChanges()) return true;

            var res = MessageBox.Show(
                "편집한 I/O·AI·CNT2 설정을 저장하시겠습니까?\n\n[예] 변경 사항 저장 후 잠금\n[아니오] 변경 사항 취소(원복) 후 잠금\n[취소] 계속 편집",
                "설정 저장 확인",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (res == MessageBoxResult.Cancel) return false;
            if (res == MessageBoxResult.Yes) vm.SaveCurrentConfigAndRecipe();
            else if (res == MessageBoxResult.No) vm.RestoreIoSnapshot();
            return true;
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

        private void BtnToggleIoLock_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm) return;
            if (vm.IsAdminUnlocked)
            {
                if (!ConfirmSaveOrRestoreIfDirty(vm))
                {
                    return;
                }
                vm.IsAdminUnlocked = false;
                return;
            }

            // 비밀번호 입력 팝업 창 (SizeToContent로 버튼 짤림 방지 + Enter/Esc 지원)
            var dlg = new Window
            {
                Title = "엔지니어 편집 잠금 해제",
                MinWidth = 350,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize
            };
            var sp = new StackPanel { Margin = new Thickness(18) };
            sp.Children.Add(new TextBlock
            {
                Text = "I/O·AI·CNT2 명칭 및 스케일 편집을 위해\n엔지니어 비밀번호를 입력하세요. (기본: 0000)",
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 12.5
            });
            var pwBox = new PasswordBox { Height = 30, Padding = new Thickness(8, 5, 8, 5), Margin = new Thickness(0, 0, 0, 14) };
            sp.Children.Add(pwBox);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnCancel = new Button { Content = "취소 (Esc)", Width = 90, Height = 32, Margin = new Thickness(0, 0, 8, 0), IsCancel = true };
            var btnOk = new Button { Content = "🔓 잠금 해제", Width = 115, Height = 32, IsDefault = true };
            btnOk.Click += (_, _) =>
            {
                if (pwBox.Password == vm.CurrentConfig.AdminPassword)
                {
                    vm.IsAdminUnlocked = true;
                    dlg.DialogResult = true;
                    dlg.Close();
                }
                else
                {
                    MessageBox.Show("비밀번호가 일치하지 않습니다.", "인증 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
                    pwBox.Clear();
                    pwBox.Focus();
                }
            };
            btnPanel.Children.Add(btnCancel);
            btnPanel.Children.Add(btnOk);
            sp.Children.Add(btnPanel);

            dlg.Content = sp;
            dlg.Loaded += (_, _) => pwBox.Focus();
            dlg.ShowDialog();
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
