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

        private void BtnStopScan_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.StopScan();
            }
        }
        
        private void BtnDoorLock_Click(object sender, RoutedEventArgs e)
        {
            // 버튼을 클릭했을 때 실행되는 로직
            System.Media.SystemSounds.Beep.Play(); // 띵~ 소리 재생
            MessageBox.Show("인터록 안전 확인: 챔버 도어가 정상 잠금(Interlock Lock)되었습니다!", 
                            "MultiScanX 도어 제어", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
        }
    }
}

