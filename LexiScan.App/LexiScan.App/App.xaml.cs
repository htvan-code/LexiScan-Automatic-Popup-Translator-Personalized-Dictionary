using System;
using System.Threading;
using System.Windows;
using LexiScan.App.Views;
using LexiScan.App.ViewModels;
using LexiScan.Core;
using LexiScan.Core.Services;
using LexiScanService;
using ScreenTranslator; // [QUAN TRỌNG] Để dùng ClipboardHookService

namespace LexiScan.App
{
    public partial class App : Application
    {
        // 1. Tạo định danh duy nhất cho App
        private const string UniqueEventName = "LexiScan_Unique_Event_Signal";
        private const string UniqueMutexName = "LexiScan_Unique_Mutex_ID";

        private EventWaitHandle _eventWaitHandle;
        private Mutex _mutex;

        // Biến lưu trữ Coordinator để dùng chung
        private AppCoordinator _coordinator;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool isOwned;
            _mutex = new Mutex(true, UniqueMutexName, out isOwned);
            _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

            if (!isOwned)
            {
                // Nếu app đã chạy rồi thì đánh thức nó dậy và tắt cái mới này đi
                _eventWaitHandle.Set();
                this.Shutdown();
                return;
            }

            base.OnStartup(e);

            // 1. Khởi tạo các Service con
            var ttsService = new TtsService();
            var voiceService = new VoicetoText();
            var translationService = new TranslationService();

            // [MỚI] 2. Khởi tạo Hook Service từ Project ScreenTranslator
            var hookService = new ClipboardHookService();

            // [SỬA] 3. Truyền tất cả vào AppCoordinator
            _coordinator = new AppCoordinator(
                translationService,
                voiceService,
                ttsService,
                hookService // <--- Truyền vào đây
            );


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
            if (loginWindow.ShowDialog() == true)
            {
                StartMainWindow();
            }
            else
            {
                Shutdown();
            }
        }

        // Hàm khởi động MainWindow
        private void StartMainWindow()
        {
            var mainVM = new MainViewModel(_coordinator);

            // Truyền coordinator vào MainWindow để nó dùng HookService
            MainWindow main = new MainWindow(_coordinator);

            main.DataContext = mainVM;

            this.MainWindow = main;

            main.Show();
        }

        public static void ChangeTheme(bool isDark)
        {
            var mergedDicts = System.Windows.Application.Current.Resources.MergedDictionaries;

            if (mergedDicts.Count > 0) mergedDicts.RemoveAt(mergedDicts.Count - 1);

            var newTheme = new ResourceDictionary();
            newTheme.Source = isDark ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                                     : new Uri("Themes/LightTheme.xaml", UriKind.Relative);
            mergedDicts.Add(newTheme);

            System.Windows.Application.Current.Resources["TextColorBrush"] = isDark
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
        }
    }
}