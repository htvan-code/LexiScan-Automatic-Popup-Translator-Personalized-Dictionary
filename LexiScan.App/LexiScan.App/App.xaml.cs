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

            // 1. Lấy Token đã lưu trong máy
            // Lưu ý: Đảm bảo bạn đã tạo biến UserToken trong Properties -> Settings như hướng dẫn trước
            string savedToken = LexiScan.App.Properties.Settings.Default.UserToken;

            // 2. Kiểm tra Token
            if (!string.IsNullOrEmpty(savedToken))
            {
                // === TRƯỜNG HỢP CÓ TOKEN: VÀO THẲNG MÀN HÌNH CHÍNH ===
                // (Bỏ qua màn hình đăng nhập)
                MainWindow main = new MainWindow();
                main.Show();
            }
            else
            {
                // === TRƯỜNG HỢP KHÔNG CÓ TOKEN: HIỆN LOGIN ===
                LoginView loginWindow = new LoginView();

                // ShowDialog sẽ chặn dòng code chạy tiếp cho đến khi cửa sổ Login đóng lại
                var result = loginWindow.ShowDialog();

                // Kiểm tra kết quả trả về từ LoginView (được set trong code-behind của LoginView)
                if (result == true)
                {
                    // Đăng nhập thành công -> Mở Main
                    MainWindow main = new MainWindow();
                    main.Show();
                }
                else
                {
                    // Người dùng tắt bảng Login -> Tắt luôn App
                    Shutdown();
                }
            }
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