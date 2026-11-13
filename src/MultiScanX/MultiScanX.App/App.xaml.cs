using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using MultiScanX.App.ViewModels;
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

            // [Windows 표준 UX 전역 등록] Ctrl+Z(실행취소), Ctrl+Y(다시실행), Ctrl+S(저장), Enter(즉시반영)
            RegisterGlobalWindowsShortcuts();

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

        private static void RegisterGlobalWindowsShortcuts()
        {
            // 1. 모든 Window(메인 + 플로팅 스튜디오 창 전체)에 Ctrl+Z / Ctrl+Y / Ctrl+S 표준 단축키 적용
            EventManager.RegisterClassHandler(typeof(Window), UIElement.PreviewKeyDownEvent, new KeyEventHandler((sender, e) =>
            {
                if (sender is not Window win) return;
                var mods = Keyboard.Modifiers;
                var vm = win.DataContext as MainViewModel ?? Current?.MainWindow?.DataContext as MainViewModel;
                if (vm == null) return;

                // Ctrl + Z (실행 취소 / Undo)
                if (e.Key == Key.Z && mods == ModifierKeys.Control)
                {
                    if (Keyboard.FocusedElement is TextBoxBase tb && !tb.IsReadOnly && tb.CanUndo)
                    {
                        return; // 입력 중인 TextBox 자체 Undo 우선
                    }
                    if (vm.Undo())
                    {
                        e.Handled = true;
                    }
                }
                // Ctrl + Y 또는 Ctrl + Shift + Z (다시 실행 / Redo)
                else if ((e.Key == Key.Y && mods == ModifierKeys.Control) ||
                         (e.Key == Key.Z && mods == (ModifierKeys.Control | ModifierKeys.Shift)))
                {
                    if (Keyboard.FocusedElement is TextBoxBase tb && !tb.IsReadOnly && tb.CanRedo)
                    {
                        return; // 입력 중인 TextBox 자체 Redo 우선
                    }
                    if (vm.Redo())
                    {
                        e.Handled = true;
                    }
                }
                // Ctrl + S (설정/레시피 저장)
                else if (e.Key == Key.S && mods == ModifierKeys.Control)
                {
                    vm.SaveCurrentConfigAndRecipe();
                    e.Handled = true;
                }
            }));

            // 2. TextBox: 한글 IME 활성화 복원(PasswordBox 이후 잠김 방지), Enter 키 즉시 반영 + 전역 Undo 기록
            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler((sender, e) =>
            {
                if (sender is TextBox tb)
                {
                    InputMethod.SetIsInputMethodEnabled(tb, true);
                    if (tb.Language == null || tb.Language.IetfLanguageTag != "ko-KR")
                    {
                        tb.Language = XmlLanguage.GetLanguage("ko-KR");
                    }
                }
            }));

            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.PreviewKeyDownEvent, new KeyEventHandler((sender, e) =>
            {
                if (sender is TextBox tb && !tb.AcceptsReturn && e.Key == Key.Enter)
                {
                    tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                    if (Window.GetWindow(tb)?.DataContext is MainViewModel vm)
                    {
                        vm.RecordUndoCheckpoint("텍스트 수정");
                    }
                }
            }));

            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler((sender, e) =>
            {
                if (sender is TextBox tb && !tb.IsReadOnly)
                {
                    tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                    if (Window.GetWindow(tb)?.DataContext is MainViewModel vm)
                    {
                        vm.RecordUndoCheckpoint("입력 변경");
                    }
                }
            }));

            // 3. ComboBox: 선택 변경 시 전역 Undo 체크포인트 기록
            EventManager.RegisterClassHandler(typeof(ComboBox), Selector.SelectionChangedEvent, new SelectionChangedEventHandler((sender, e) =>
            {
                if (sender is ComboBox cb && cb.IsDropDownOpen)
                {
                    cb.Dispatcher.BeginInvoke(() =>
                    {
                        if (Window.GetWindow(cb)?.DataContext is MainViewModel vm)
                        {
                            vm.RecordUndoCheckpoint("옵션 선택 변경");
                        }
                    });
                }
            }));
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
