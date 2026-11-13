using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MultiScanX.App.ViewModels;
using MultiScanX.App.Views.Panels;

namespace MultiScanX.App.Views
{
    public partial class LayoutCWindow : Window
    {
        private bool _isDrawerOpen = false;

        public LayoutCWindow()
        {
            InitializeComponent();
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

        private void BtnPopupRecipe_Click(object sender, RoutedEventArgs e)
        {
            ShowStudioPopup("📐 10×10 마스터 레시피 편집 & 행/열 미세조정 스튜디오 (Floating Window)", new RecipeEditorControl(), 1280, 860);
        }

        private void BtnPopupIoCheck_Click(object sender, RoutedEventArgs e)
        {
            ShowStudioPopup("🛠 H/W & I/O Check 스튜디오 — DB32T (16DI/16DO) / AI16 (16CH 아날로그) / CNT2 (Z-Scale Low-Level) / CI-600A", new IoHardwareCheckControl(), 1560, 900);
        }

        private void BtnPopupConfig_Click(object sender, RoutedEventArgs e)
        {
            ShowStudioPopup("⚙ 3종 센서 3D 오프셋 & Z축 다중 정지 우선순위 / CI-600A 설정 스튜디오", new SystemConfigControl(), 1240, 820);
        }

        private void ShowStudioPopup(string title, UIElement contentControl, double width, double height)
        {
            var popup = new Window
            {
                Title = title,
                Width = width,
                Height = height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4F7FB")),
                DataContext = this.DataContext,
                Owner = this,
                Content = new Border
                {
                    Padding = new Thickness(14),
                    Child = contentControl
                }
            };
            popup.Show();
        }

        private void BtnToggleDrawer_Click(object sender, RoutedEventArgs e)
        {
            _isDrawerOpen = !_isDrawerOpen;
            if (_isDrawerOpen)
            {
                DrawerCol.Width = new GridLength(520);
                QuickDrawerBorder.Visibility = Visibility.Visible;
                BtnToggleDrawer.Content = "📌 우측 퀵드로어 닫기";
            }
            else
            {
                DrawerCol.Width = new GridLength(0);
                QuickDrawerBorder.Visibility = Visibility.Collapsed;
                BtnToggleDrawer.Content = "📌 우측 퀵드로어 열기";
            }
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

        private void BtnOpenLayoutA_Click(object sender, RoutedEventArgs e)
        {
            var win = new LayoutAWindow { DataContext = this.DataContext };
            win.Show();
        }

        private void BtnOpenLayoutB_Click(object sender, RoutedEventArgs e)
        {
            var win = new LayoutBWindow { DataContext = this.DataContext };
            win.Show();
        }
    }
}

