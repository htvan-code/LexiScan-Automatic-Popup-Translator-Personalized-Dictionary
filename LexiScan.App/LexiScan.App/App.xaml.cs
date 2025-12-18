using System;
using System.Windows;
using LexiScan.App.Views; // Để gọi LoginView

namespace LexiScan.App
{
    public partial class App : Application
    {
        // ============================================================
        // PHẦN 1: XỬ LÝ KHỞI ĐỘNG & TỰ ĐỘNG ĐĂNG NHẬP (MỚI THÊM)
        // ============================================================
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // QUAN TRỌNG: Ngăn app tự tắt khi đóng LoginView để chuyển sang MainWindow
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Kiểm tra ghi nhớ tài khoản
            string savedToken = LexiScan.App.Properties.Settings.Default.UserToken;

            if (!string.IsNullOrEmpty(savedToken))
            {
                StartMainWindow();
            }
            else
            {
                ShowLoginWindow();
            }
        }

        public void ShowLoginWindow()
        {
            LoginView loginWindow = new LoginView();
            // Nếu LoginViewModel gọi window.DialogResult = true
            if (loginWindow.ShowDialog() == true)
            {
                StartMainWindow();
            }
            else
            {
                Shutdown(); // Thoát hẳn nếu người dùng tắt bảng login
            }
        }

        private void StartMainWindow()
        {
            MainWindow main = new MainWindow();
            main.Show();
        }
        // ============================================================
        // PHẦN 2: HÀM ĐỔI THEME (CODE CŨ CỦA BẠN - GIỮ NGUYÊN)
        // ============================================================
        public static void ChangeTheme(bool isDark)
        {
            var mergedDicts = Current.Resources.MergedDictionaries;

            // Xóa theme cũ
            if (mergedDicts.Count > 0)
            {
                // Giả định theme tùy chỉnh của bạn luôn nằm ở cuối cùng
                mergedDicts.RemoveAt(mergedDicts.Count - 1);
            }

            // Thêm theme mới
            var newTheme = new ResourceDictionary();
            if (isDark)
            {
                newTheme.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
            }
            else
            {
                newTheme.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
            }

            mergedDicts.Add(newTheme);
        }
    }
}