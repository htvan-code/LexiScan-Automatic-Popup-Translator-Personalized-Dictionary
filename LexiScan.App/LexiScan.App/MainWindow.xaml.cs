using LexiScan.App.Services;
using LexiScan.App.ViewModels; 
using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
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
        private ClipboardHookService _hookService;
        private readonly AppCoordinator _coordinator; 
        private PopupView _popupWindow;

        private int _currentKey = 0x20; // Space
        private int _currentMod = ClipboardHookService.MOD_CONTROL;

        public MainWindow(AppCoordinator coordinator)
        {
            InitializeComponent();

            RestoreSession();

            _coordinator = coordinator;
            _coordinator.OnPopupResultReceived += OnTranslationResultReceived;

            _hookService = new ClipboardHookService();
            _hookService.OnTextCaptured += SendTextToCoordinator;
            _hookService.OnError += (msg) => MessageBox.Show(msg);

            _trayService = new TrayService(this);
            _trayService.Initialize();
        }


        // PHẦN 1: ĐĂNG KÝ HOTKEY 
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;
            try
            {
                _hookService.Register(handle, _currentMod, _currentKey);
                HwndSource source = HwndSource.FromHwnd(handle);
                source.AddHook(HwndHook);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng ký Hotkey: " + ex.Message);
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            _hookService.ProcessWindowMessage(msg, (int)wParam);
            return IntPtr.Zero;
        }

        // PHẦN 2: XỬ LÝ DỊCH VÀ HIỆN POPUP 
        private async void SendTextToCoordinator(string text)
        {
            var settingsService = new LexiScan.Core.Services.SettingsService();
            var settings = settingsService.LoadSettings();

            if (!settings.IsAutoReadEnabled) return;

            if (!string.IsNullOrWhiteSpace(text))
            {
                await _coordinator.HandleClipboardTextAsync(text);
            }
        }
        private void OnTranslationResultReceived(TranslationResult result)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (!result.IsFromClipboard)
                {
                    return;
                }

                var settingsService = new LexiScan.Core.Services.SettingsService();
                var settings = settingsService.LoadSettings();

                if (!settings.IsAutoReadEnabled)
                {
                    return;
                }

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

        private bool IsWindowOpen(Window window)
        {
            return window.IsLoaded && window.Visibility != Visibility.Collapsed;
        }

        // PHẦN 3: ĐÓNG/ẨN CỬA SỔ 
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
                }
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
    }
}