using System.Windows;
using System.Windows.Controls;
using MultiScanX.App.ViewModels;
using MultiScanX.Core.Models;

namespace MultiScanX.App.Views.Panels
{
    public partial class StageMapControl : UserControl
    {
        public StageMapControl()
        {
            InitializeComponent();
        }

        private void PointPin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is InspectionPoint pt && DataContext is MainViewModel vm)
            {
                vm.SelectedPoint = pt;
                vm.StatusMessage = $"[포인트 선택] Pt #{pt.PointId} ({pt.GridLabel}) | X={pt.X:F1}mm, Y={pt.Y:F1}mm, TargetZ={pt.TargetZ:F3}mm";
            }
        }

        private void BtnMoveToSelected_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.MoveToSelectedPoint();
            }
        }

        private void BtnMeasureSelected_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.MeasureSelectedPointOnce();
            }
        }

        private void BtnF20ZeroCal_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ExecuteF20ZeroCalibration();
            }
        }
    }
}

