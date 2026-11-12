using System.Windows;
using System.Windows.Controls;
using MultiScanX.App.ViewModels;

namespace MultiScanX.App.Views.Panels
{
    public partial class SystemConfigControl : UserControl
    {
        public SystemConfigControl()
        {
            InitializeComponent();
        }

        private void BtnToggleUnlock_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                if (vm.IsAdminUnlocked)
                {
                    vm.IsAdminUnlocked = false;
                    vm.StatusMessage = "🔒 엔지니어 설정이 잠금(Lock) 상태로 전환되었습니다.";
                }
                else if (PwdBox.Password == vm.CurrentConfig.AdminPassword)
                {
                    vm.IsAdminUnlocked = true;
                    vm.StatusMessage = "🔓 엔지니어 비밀번호 인증 성공! 파라미터 수정 및 저장이 활성화되었습니다.";
                }
                else
                {
                    MessageBox.Show("비밀번호가 일치하지 않습니다. (기본 비밀번호: 0000)", "인증 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SaveCurrentConfigAndRecipe();
            }
        }

        private void BtnExecF20Cal_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ExecuteF20ZeroCalibration();
            }
        }
    }
}

