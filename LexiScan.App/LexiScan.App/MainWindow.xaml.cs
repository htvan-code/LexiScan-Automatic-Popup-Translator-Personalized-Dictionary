using LexiScan.App.Services;
using LexiScan.App.ViewModels; // Không cần using Service lẻ tẻ nữa
using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
using LexiScanUI.View;
using LexiScanUI.ViewModels;
using ScreenTranslator;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

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

            // [SỬA]: Không dùng TranslationCompleted nữa vì nó dùng chung cho cả Dictionary
            // Đổi sang sự kiện OnPopupResultReceived chuyên biệt cho Popup
            _coordinator.OnPopupResultReceived += OnTranslationResultReceived;

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
        // Hàm này chạy khi bạn bấm Ctrl + Space (hoặc Hotkey đã cài)
        private async void SendTextToCoordinator(string text)
        {
            // [SỬA]: Khởi tạo service để load cài đặt
            var settingsService = new LexiScan.Core.Services.SettingsService();
            var settings = settingsService.LoadSettings();

            // Nếu tắt Popup thì dừng hẳn, không gửi yêu cầu dịch đi đâu cả
            if (!settings.IsAutoReadEnabled) return;

            if (!string.IsNullOrWhiteSpace(text))
            {
                // Gửi văn bản bôi đen sang Coordinator để xử lý riêng cho Popup
                await _coordinator.HandleClipboardTextAsync(text);
            }
        }
        private void OnTranslationResultReceived(TranslationResult result)
        {
            this.Dispatcher.Invoke(() =>
            {
                // 1. Kiểm tra nguồn (Chỉ hiện popup nếu từ Clipboard/Bôi đen)
                if (!result.IsFromClipboard)
                {
                    return;
                }

                // 2. [MỚI] KIỂM TRA CÀI ĐẶT BẬT/TẮT POPUP
                // Phải khai báo namespace: using LexiScan.Core.Services; ở đầu file
                var settingsService = new LexiScan.Core.Services.SettingsService();
                var settings = settingsService.LoadSettings();

                // Nếu người dùng đã bỏ tích "Bật/Tắt Popup" -> Dừng lại, KHÔNG hiện cửa sổ
                if (!settings.IsAutoReadEnabled)
                {
                    return;
                }

                // 3. Nếu mọi thứ OK -> Hiện Popup
                if (result.Status == ServiceStatus.Success)
                {
                    if (_popupWindow == null || !IsWindowOpen(_popupWindow))
                    {
                        _popupWindow = new PopupView();
                    }

                    _popupWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    _popupWindow.Topmost = true;

                    _popupWindow.Show(); // <--- Dòng này sẽ không chạy nếu bị return ở trên
                    _popupWindow.Activate();

                    // Truyền dữ liệu vào ViewModel của Popup
                    if (_popupWindow.DataContext is PopupViewModel vm)
                    {
                        vm.LoadTranslationData(result);
                    }
                    // Hoặc dùng cách cũ của bạn: _popupWindow.ShowResult(result);
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