using System;
using System.IO;
using System.Windows;

namespace MultiScanX.App
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // [Auto-Provisioning] 프로그램 최초 실행 시 필요한 데이터/로그/레시피 디렉토리 자동 생성
            EnsureRequiredDirectoriesExist();
        }

        private static void EnsureRequiredDirectoriesExist()
        {
            try
            {
                // D: 드라이브 존재 여부 확인 후 자동 생성, 없으면 C: 드라이브로 폴백
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
                System.Diagnostics.Debug.WriteLine($"[디렉토리 자동생성 오류] {ex.Message}");
            }
        }
    }
}
