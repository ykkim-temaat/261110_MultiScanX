using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using MultiScanX.App.Views;

namespace MultiScanX.App
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += (s, exArgs) =>
            {
                try { File.WriteAllText(@"C:\MultiScanX\config\last_crash.log", exArgs.Exception.ToString()); } catch { }
                MessageBox.Show($"[MultiScan-X Studio 실행 오류]\n{exArgs.Exception.Message}\n\n{exArgs.Exception.InnerException?.Message}",
                    "MultiScan-X Studio Error", MessageBoxButton.OK, MessageBoxImage.Error);
                exArgs.Handled = true;
            };

            base.OnStartup(e);

            // [Auto-Provisioning] 프로그램 최초 실행 시 필요한 데이터/로그/레시피 디렉토리 자동 생성
            EnsureRequiredDirectoriesExist();

            // 컴파일 심볼(LAYOUT_A / LAYOUT_B / LAYOUT_C), 실행 파일명, 또는 CLI 인자에 따라 3종 GUI 레이아웃 분기
            Window mainWin = ResolveStartupLayoutWindow(e.Args);
            MainWindow = mainWin;
            mainWin.Show();
        }

        private static Window ResolveStartupLayoutWindow(string[] args)
        {
            string exeName = Path.GetFileNameWithoutExtension(Environment.ProcessPath ?? string.Empty).ToUpperInvariant();
            string argStr = string.Join(" ", args).ToUpperInvariant();

            if (argStr.Contains("LAYOUTB") || exeName.Contains("LAYOUTB"))
            {
                return new LayoutBWindow();
            }
            if (argStr.Contains("LAYOUTC") || exeName.Contains("LAYOUTC"))
            {
                return new LayoutCWindow();
            }
            if (argStr.Contains("LAYOUTA") || exeName.Contains("LAYOUTA"))
            {
                return new LayoutAWindow();
            }

#if LAYOUT_A
            return new LayoutAWindow();
#elif LAYOUT_B
            return new LayoutBWindow();
#else
            // 최종 확정 GUI 레이아웃: 초안 C (미니멀 메인 관제 데스크 + 독립 플로팅 스튜디오 팝업형)
            return new LayoutCWindow();
#endif
        }

        private static void EnsureRequiredDirectoriesExist()
        {
            try
            {
                string dataRoot = Directory.Exists(@"D:\") ? @"D:\MultiScanX_Data" : @"C:\MultiScanX_Data";

                string[] requiredDirs = new[]
                {
                    Path.Combine(dataRoot, "Results"),
                    Path.Combine(dataRoot, "Maps"),
                    Path.Combine(dataRoot, "Logs"),
                    Path.Combine(dataRoot, "Recipes"),
                    @"C:\MultiScanX\config",
                    @"C:\MultiScanX\versions"
                };

                foreach (var dir in requiredDirs)
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[디렉토리 자동생성 오류] {ex.Message}");
            }
        }
    }
}
