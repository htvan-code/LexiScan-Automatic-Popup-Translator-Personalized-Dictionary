using LexiScan.App.Services;
using LexiScan.App.ViewModels;
using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services; // Cần namespace này cho SettingsService
using LexiScanUI.View;
using LexiScanUI.ViewModels;
using ScreenTranslator;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Navigation;

namespace LexiScan.App
{
    public partial class MainWindow : Window
    {
        private TrayService _trayService;
        private readonly AppCoordinator _coordinator;
        private PopupView _popupWindow;

        // [ĐÃ XÓA] Các biến _currentKey, _currentMod, _hookService không cần nữa
        // vì logic Hotkey giờ được quản lý tập trung trong Service.

        public MainWindow(AppCoordinator coordinator)
        {
            InitializeComponent();

            RestoreSession();

            _coordinator = coordinator;

            // Lắng nghe kết quả dịch để hiển thị Popup
            _coordinator.OnPopupResultReceived += OnTranslationResultReceived;

            // [SỬA] Không tạo mới HookService ở đây nữa.
            // AppCoordinator đã giữ instance của HookService rồi.

            _trayService = new TrayService(this);
            _trayService.Initialize();
        }

        // PHẦN 1: ĐĂNG KÝ HOTKEY & WINDOW HOOK
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var handle = new WindowInteropHelper(this).Handle;

            try
            {
                // 1. Đăng ký Handle
                _coordinator.HookService.Register(handle);

                // [MỚI] Lắng nghe lỗi để debug
                _coordinator.HookService.OnError += (msg) =>
                {
                    MessageBox.Show("Lỗi Hotkey Startup: " + msg);
                };

                // 2. Load settings và kích hoạt
                var settingsService = new SettingsService();
                var settings = settingsService.LoadSettings();

                // Kiểm tra xem settings có trống không
                if (!string.IsNullOrEmpty(settings.Hotkey))
                {
                    _coordinator.HookService.UpdateHotkey(settings.Hotkey);
                }
                else
                {
                    // Nếu trống thì gán mặc định Ctrl + Space
                    _coordinator.HookService.UpdateHotkey("Ctrl + Space");
                }

                // 3. Hook Window Proc
                HwndSource source = HwndSource.FromHwnd(handle);
                source?.AddHook(WndProc);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo: " + ex.Message);
            }
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // [SỬA] Xóa .ToInt32(), truyền trực tiếp wParam
            _coordinator.HookService.ProcessWindowMessage(msg, wParam);

            return IntPtr.Zero;
        }

        // PHẦN 2: XỬ LÝ HIỆN POPUP
        // [ĐÃ XÓA] Hàm SendTextToCoordinator không cần nữa vì AppCoordinator tự nghe event

        private void OnTranslationResultReceived(TranslationResult result)
        {
            this.Dispatcher.Invoke(() =>
            {
                // Chỉ xử lý nếu là dịch từ Clipboard/Hotkey
                if (!result.IsFromClipboard) return;

                // Kiểm tra cài đặt xem có cho phép tự động hiện không
                var settingsService = new SettingsService();
                var settings = settingsService.LoadSettings();

                if (!settings.IsAutoReadEnabled) return;

                if (result.Status == ServiceStatus.Success)
                {
                    if (_popupWindow == null || !IsWindowOpen(_popupWindow))
                    {
                        _popupWindow = new PopupView();
                    }

                    _popupWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    _popupWindow.Topmost = true;

                    _popupWindow.Show();
                    _popupWindow.Activate();

                    if (_popupWindow.DataContext is PopupViewModel vm)
                    {
                        vm.LoadTranslationData(result);
                    }
                }
            });
        }

        // ... (CÁC PHẦN DƯỚI GIỮ NGUYÊN) ...

        private bool IsWindowOpen(Window window)
        {
            return window.IsLoaded && window.Visibility != Visibility.Collapsed;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            if (_popupWindow != null && IsWindowOpen(_popupWindow))
            {
                _popupWindow.Close();
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        public void ShowMainWindow()
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
            this.Visibility = Visibility.Visible;
            this.Show();
            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private async void RestoreSession()
        {
            try
            {
                string savedUid = LexiScan.App.Properties.Settings.Default.UserId;
                string savedRefreshToken = LexiScan.App.Properties.Settings.Default.RefreshToken;

                if (!string.IsNullOrEmpty(savedUid) && !string.IsNullOrEmpty(savedRefreshToken))
                {
                    LexiScan.Core.SessionManager.CurrentUserId = savedUid;

                    var authService = new LexiScan.Core.Services.AuthService();
                    string newToken = await authService.AutoLoginAsync(savedRefreshToken);

                    if (!string.IsNullOrEmpty(newToken))
                    {
                        LexiScan.Core.SessionManager.CurrentAuthToken = newToken;
                        Dispatcher.Invoke(() =>
                        {
                            LexiScan.Core.Utils.GlobalEvents.RaisePersonalDictionaryUpdated();
                        });
                    }
                }
            }
            catch (Exception) { /* Bỏ qua lỗi session */ }
        }
    }
}