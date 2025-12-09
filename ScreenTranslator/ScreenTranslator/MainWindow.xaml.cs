using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Input;

namespace ScreenTranslator
{
    public partial class MainWindow : Window
    {
        // Khai báo Service (Cần câu)
        private ClipboardHookService _hookService;
        private bool _hasSetPosition = false;

        private int _currentMod = ClipboardHookService.MOD_CONTROL;
        private int _currentKey = 0x20;

        public MainWindow()
        {
            InitializeComponent();

            // 1. Khởi tạo Service
            _hookService = new ClipboardHookService();

            // 2. Đăng ký lắng nghe sự kiện
            // Khi Service bắt được chữ -> Gọi hàm ShowResult
            _hookService.OnTextCaptured += ShowResult;

            // Khi Service báo lỗi -> Gọi hàm ShowError
            _hookService.OnError += ShowError;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;

            // 3. Nhờ Service đăng ký Hotkey
            _hookService.Register(handle, _currentMod, _currentKey);

            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // 4. Khi có tin nhắn, chuyển ngay cho Service xử lý
            _hookService.ProcessWindowMessage(msg, wParam.ToInt32());

            return IntPtr.Zero;
        }

        // --- CÁC HÀM HIỂN THỊ GIAO DIỆN (UI) ---

        private void ShowResult(string text)
        {
            txtResult.Text = text;
            ShowPopupMemory();
        }

        private void ShowError(string errorMsg)
        {
            txtResult.Text = "Lỗi: " + errorMsg;
            ShowPopupMemory();
        }

        private void ShowPopupMemory()
        {
            this.Visibility = Visibility.Visible;
            this.UpdateLayout();

            if (!_hasSetPosition)
            {
                var screenHeight = System.Windows.SystemParameters.WorkArea.Height;
                var windowHeight = this.ActualHeight > 0 ? this.ActualHeight : 100;

                this.Left = 20;
                this.Top = screenHeight - windowHeight - 20;
                _hasSetPosition = true;
            }

            this.Topmost = true;
            this.Activate();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        protected override void OnClosed(EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            // Nhờ Service dọn dẹp
            _hookService.Unregister();
            base.OnClosed(e);
        }
    }
}