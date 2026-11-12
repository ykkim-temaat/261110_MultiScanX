using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MultiScanX.App.ViewModels;

namespace MultiScanX.App.Views.Panels
{
    public partial class RecipeEditorControl : UserControl
    {
        public RecipeEditorControl()
        {
            InitializeComponent();
        }

        private void BtnLoad10x10_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SwitchRecipePreset(true);
            }
        }

        private void BtnLoad30Pt_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SwitchRecipePreset(false);
            }
        }

        private void BtnOptimizeSnake_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.OptimizeRecipePath();
            }
        }

        private void BtnSaveRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SaveCurrentConfigAndRecipe();
            }
        }

        private void BtnShift_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string cmd && DataContext is MainViewModel vm)
            {
                var parts = cmd.Split(':');
                if (parts.Length == 2 && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double sign))
                {
                    if (parts[0] == "X") vm.ShiftRecipePoints(sign * vm.ShiftDeltaMm, 0, 0);
                    else if (parts[0] == "Y") vm.ShiftRecipePoints(0, sign * vm.ShiftDeltaMm, 0);
                    else if (parts[0] == "Z") vm.ShiftRecipePoints(0, 0, sign * vm.ShiftDeltaZMm);
                }
            }
        }

        private void BtnMoveToSelectedRow_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.MoveToSelectedPoint();
            }
        }

        private void BtnMeasureSelectedRow_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.MeasureSelectedPointOnce();
            }
        }
    }
}

