using System;
using System.Threading; // Để dùng EventWaitHandle và Thread
using System.Windows;
using LexiScan.App.Views;
using LexiScan.App.ViewModels; // [THÊM] Để dùng MainViewModel
using LexiScan.Core;           // [THÊM] Để dùng AppCoordinator
using LexiScan.Core.Services;
using LexiScanService;  // [THÊM] Để dùng các Services

namespace LexiScan.App
{
    public partial class App : Application
    {
        // 1. Tạo định danh duy nhất cho App
        private const string UniqueEventName = "LexiScan_Unique_Event_Signal";
        private const string UniqueMutexName = "LexiScan_Unique_Mutex_ID";

        private EventWaitHandle _eventWaitHandle;
        private Mutex _mutex;

        // [THÊM] Biến lưu trữ Coordinator để dùng chung
        private AppCoordinator _coordinator;

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

            // [QUAN TRỌNG - BƯỚC 1] Khởi tạo hệ thống Service & Coordinator TẠI ĐÂY
            // Chúng ta phải tạo nó trước khi mở bất kỳ cửa sổ nào
            var ttsService = new TtsService();
            var voiceService = new VoicetoText();
            var translationService = new TranslationService();

            _coordinator = new AppCoordinator(translationService, voiceService, ttsService);


            // A. Tạo luồng lắng nghe tín hiệu (để chờ bị đánh thức)
            StartSignalListener();

            // B. Code khởi động cũ của bạn
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Kiểm tra Token đăng nhập
            string savedToken = LexiScan.App.Properties.Settings.Default.UserId;
            if (!string.IsNullOrEmpty(savedToken))
            {
                StartMainWindow();
                SessionManager.CurrentUserId = savedToken;
            }
            else
            {
                ShowLoginWindow();
            }
        }

        // --- HÀM LẮNG NGHE TÍN HIỆU TỪ APP KHÁC (Giữ nguyên) ---
        private void StartSignalListener()
        {
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    _eventWaitHandle.WaitOne();
                    this.Dispatcher.Invoke(() =>
                    {
                        var mainWindow = this.MainWindow as MainWindow;
                        if (mainWindow != null)
                        {
                            mainWindow.ShowMainWindow();
                        }
                    });
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        public void ShowLoginWindow()
        {
            LoginView loginWindow = new LoginView();
            // LoginView có thể cần ViewModel riêng hoặc xử lý code-behind đơn giản
            // Nếu Login thành công thì gọi StartMainWindow()
            if (loginWindow.ShowDialog() == true)
            {
                StartMainWindow();
            }
            else
            {
                Shutdown();
            }
        }

        // [QUAN TRỌNG - BƯỚC 2] Sửa hàm khởi động MainWindow
        private void StartMainWindow()
        {
            // Tạo ViewModel và truyền coordinator vào
            var mainVM = new MainViewModel(_coordinator);

            // Tạo MainWindow và truyền coordinator vào (để dùng cho Hotkey/Clipboard)
            MainWindow main = new MainWindow(_coordinator);

            // Gán DataContext
            main.DataContext = mainVM;

            // Gán MainWindow của Application để luồng lắng nghe có thể tìm thấy
            this.MainWindow = main;

            main.Show();
        }

        // Hàm đổi Theme (Giữ nguyên)
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