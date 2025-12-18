using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using LexiScan.App.ViewModels; // Không cần using Service lẻ tẻ nữa
using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScanUI.View;
using ScreenTranslator;

namespace LexiScan.App
{
    public partial class MainWindow : Window
    {
        // Chỉ giữ lại các biến để dùng nội bộ
        private TrayService _trayService;
        private ClipboardHookService _hookService;
        private readonly AppCoordinator _coordinator; // readonly vì chỉ nhận 1 lần
        private PopupView _popupWindow;

        // Cấu hình phím tắt
        private int _currentKey = 0x20; // Space
        private int _currentMod = ClipboardHookService.MOD_CONTROL;

        // [QUAN TRỌNG] Constructor nhận AppCoordinator từ App.xaml.cs
        public MainWindow(AppCoordinator coordinator)
        {
            InitializeComponent();

            // 1. Nhận Coordinator được truyền vào
            _coordinator = coordinator;

            // Đăng ký sự kiện nhận kết quả dịch để hiện Popup
            _coordinator.TranslationCompleted += OnTranslationResultReceived;

            // 2. Các Service liên quan mật thiết đến Window (Hook & Tray) thì giữ lại ở đây cũng được

            // B. Dịch vụ bắt phím tắt & Clipboard
            _hookService = new ClipboardHookService();
            _hookService.OnTextCaptured += SendTextToCoordinator;
            _hookService.OnError += (msg) => MessageBox.Show(msg);

            // C. Dịch vụ Khay hệ thống
            _trayService = new TrayService(this);
            _trayService.Initialize();
        }

        // --- CÁC PHẦN CÒN LẠI GIỮ NGUYÊN ---

        // PHẦN 1: ĐĂNG KÝ HOTKEY (Giữ nguyên)
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

        // PHẦN 2: XỬ LÝ DỊCH VÀ HIỆN POPUP (Giữ nguyên)
        private async void SendTextToCoordinator(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                // Gọi coordinator đã được tiêm vào
                await _coordinator.HandleClipboardTextAsync(text);
            }
        }

        private void OnTranslationResultReceived(TranslationResult result)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (result.Status == ServiceStatus.Success)
                {
                    // [SỬA] COMMENT HOẶC XÓA ĐOẠN CODE NÀY ĐI
                    // Vì DictionaryView đã tự lo việc hiển thị rồi, không cần MainWindow bật popup nữa.

                    /* if (_popupWindow == null || !IsWindowOpen(_popupWindow))
                    {
                        _popupWindow = new PopupView();
                    }

                    _popupWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    _popupWindow.Topmost = true;
                    _popupWindow.Show();
                    _popupWindow.Activate();
                    _popupWindow.ShowResult(result);
                    */
                }
            });
        }

        private bool IsWindowOpen(Window window)
        {
            return window.IsLoaded && window.Visibility != Visibility.Collapsed;
        }

        // PHẦN 3: ĐÓNG/ẨN CỬA SỔ (Giữ nguyên)
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
            // Trick để focus
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }
    }
}