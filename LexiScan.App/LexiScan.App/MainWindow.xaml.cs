using System;
using System.ComponentModel; // [CẦN THÊM] Để dùng CancelEventArgs
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop; // [CẦN THÊM] Để xử lý Hotkey (HwndSource)
using LexiScan.App.ViewModels;
using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
using LexiScanUI.View; // [CẦN THÊM] Để gọi PopupView
using ScreenTranslator;

namespace LexiScan.App
{
    public partial class MainWindow : Window
    {
        // 1. Các Service cần thiết
        private TrayService _trayService;
        private ClipboardHookService _hookService;
        private AppCoordinator _coordinator;
        private PopupView _popupWindow;

        // 2. Cấu hình phím tắt (Ví dụ: Ctrl + Space)
        // 0x20 là Space, 0x51 là Q. Nếu Space bị trùng thì đổi sang Q nhé.
        private int _currentKey = 0x20;
        private int _currentMod = ClipboardHookService.MOD_CONTROL;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();

            // --- KHỞI TẠO CÁC SERVICE ---

            // A. Dịch vụ dịch thuật & điều phối
            var transService = new TranslationService();
            var voiceService = new VoicetoText();
            var ttsService = new TtsService();
            _coordinator = new AppCoordinator(transService, voiceService, ttsService);
            _coordinator.TranslationCompleted += OnTranslationResultReceived;

            // B. Dịch vụ bắt phím tắt & Clipboard
            _hookService = new ClipboardHookService();
            _hookService.OnTextCaptured += SendTextToCoordinator;
            _hookService.OnError += (msg) => MessageBox.Show(msg);

            // C. Dịch vụ Khay hệ thống (Tray Icon)
            _trayService = new TrayService(this);
            _trayService.Initialize();
        }

        // ============================================================
        // PHẦN 1: ĐĂNG KÝ HOTKEY VỚI WINDOWS (QUAN TRỌNG NHẤT)
        // ============================================================
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Lấy Handle (mã định danh) của cửa sổ này
            IntPtr handle = new WindowInteropHelper(this).Handle;

            try
            {
                // Đăng ký Hotkey
                _hookService.Register(handle, _currentMod, _currentKey);

                // Thêm Hook để lắng nghe tin nhắn từ hệ điều hành
                HwndSource source = HwndSource.FromHwnd(handle);
                source.AddHook(HwndHook);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng ký Hotkey: " + ex.Message);
            }
        }

        // Hàm lọc tin nhắn hệ thống để bắt sự kiện bấm phím
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            _hookService.ProcessWindowMessage(msg, (int)wParam);
            return IntPtr.Zero;
        }

        // ============================================================
        // PHẦN 2: XỬ LÝ DỊCH VÀ HIỆN POPUP
        // ============================================================

        // Khi bắt được text từ Clipboard -> Gửi đi dịch
        private async void SendTextToCoordinator(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                await _coordinator.HandleClipboardTextAsync(text);
            }
        }

        // Khi có kết quả dịch -> Hiện Popup
        private void OnTranslationResultReceived(TranslationResult result)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (result.Status == ServiceStatus.Success)
                {
                    // Tạo mới Popup nếu chưa có hoặc đã bị đóng
                    if (_popupWindow == null || !IsWindowOpen(_popupWindow))
                    {
                        _popupWindow = new PopupView();
                    }

                    // Cài đặt hiển thị: Giữa màn hình & Luôn ở trên cùng
                    _popupWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    _popupWindow.Topmost = true;

                    // [QUAN TRỌNG] Phải Show() và Activate() để popup đè lên các cửa sổ khác
                    _popupWindow.Show();
                    _popupWindow.Activate();

                    // Đổ dữ liệu vào
                    _popupWindow.ShowResult(result);
                }
            });
        }

        // Hàm kiểm tra cửa sổ còn sống hay không
        private bool IsWindowOpen(Window window)
        {
            return window.IsLoaded && window.Visibility != Visibility.Collapsed;
        }

        // ============================================================
        // PHẦN 3: XỬ LÝ ĐÓNG/ẨN CỬA SỔ
        // ============================================================

        // Sự kiện khi bấm nút X trên thanh tiêu đề hoặc Alt+F4
        protected override void OnClosing(CancelEventArgs e)
        {
            // Hủy lệnh đóng thật -> Chỉ ẩn đi
            e.Cancel = true;
            this.Hide();

            // Nếu có popup đang mở thì đóng nó đi cho gọn
            if (_popupWindow != null && IsWindowOpen(_popupWindow))
            {
                _popupWindow.Close();
            }
        }

        // Nút đóng trên giao diện (nếu có)
        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        // Kéo thả cửa sổ
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        // Trong file MainWindow.xaml.cs
        public void ShowMainWindow()
        {
            // 1. Khôi phục kích thước nếu đang thu nhỏ
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }

            // 2. Hiện cửa sổ
            this.Visibility = Visibility.Visible;
            this.Show();

            // 3. Đưa lên trên cùng
            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }
    }
}