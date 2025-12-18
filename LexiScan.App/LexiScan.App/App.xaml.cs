using System;
using System.Threading; // [QUAN TRỌNG] Để dùng EventWaitHandle và Thread
using System.Windows;
using LexiScan.App.Views;

namespace LexiScan.App
{
    public partial class App : Application
    {
        // 1. Tạo định danh duy nhất cho App
        private const string UniqueEventName = "LexiScan_Unique_Event_Signal";
        private const string UniqueMutexName = "LexiScan_Unique_Mutex_ID";

        private EventWaitHandle _eventWaitHandle;
        private Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool isOwned;
            _mutex = new Mutex(true, UniqueMutexName, out isOwned);
            _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

            // ============================================================
            // TRƯỜNG HỢP 1: APP ĐÃ CHẠY TỪ TRƯỚC (BẢN CŨ)
            // ============================================================
            if (!isOwned)
            {
                // Gửi tín hiệu đánh thức cho bản cũ
                _eventWaitHandle.Set();

                // Tắt bản mới này ngay lập tức
                this.Shutdown();
                return;
            }

            // ============================================================
            // TRƯỜNG HỢP 2: ĐÂY LÀ BẢN ĐẦU TIÊN (CHƯA CHẠY)
            // ============================================================
            base.OnStartup(e);

            // A. Tạo luồng lắng nghe tín hiệu (để chờ bị đánh thức)
            StartSignalListener();

            // B. Code khởi động cũ của bạn
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Kiểm tra Token đăng nhập
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

        // --- HÀM LẮNG NGHE TÍN HIỆU TỪ APP KHÁC ---
        private void StartSignalListener()
        {
            // Tạo một luồng riêng để chờ tín hiệu, tránh treo giao diện chính
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    // Chờ tín hiệu từ bản copy khác gửi tới
                    _eventWaitHandle.WaitOne();

                    // Khi nhận được tín hiệu -> Nhờ Dispatcher (luồng chính) mở giao diện
                    this.Dispatcher.Invoke(() =>
                    {
                        var mainWindow = this.MainWindow as MainWindow;
                        if (mainWindow != null)
                        {
                            // Gọi hàm hiển thị trong MainWindow
                            mainWindow.ShowMainWindow();
                        }
                    });
                }
            });

            thread.IsBackground = true; // Để luồng này tự tắt khi App tắt
            thread.Start();
        }

        public void ShowLoginWindow()
        {
            LoginView loginWindow = new LoginView();
            if (loginWindow.ShowDialog() == true)
            {
                StartMainWindow();
            }
            else
            {
                Shutdown();
            }
        }

        private void StartMainWindow()
        {
            MainWindow main = new MainWindow();

            // Gán MainWindow của Application để luồng lắng nghe có thể tìm thấy
            this.MainWindow = main;

            main.Show();
        }

        // Hàm đổi Theme cũ giữ nguyên
        public static void ChangeTheme(bool isDark)
        {
            var mergedDicts = Current.Resources.MergedDictionaries;
            if (mergedDicts.Count > 0) mergedDicts.RemoveAt(mergedDicts.Count - 1);
            var newTheme = new ResourceDictionary();
            newTheme.Source = isDark ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative) : new Uri("Themes/LightTheme.xaml", UriKind.Relative);
            mergedDicts.Add(newTheme);
        }
    }
}