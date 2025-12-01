using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Input; // Cần thư viện này để xử lý chuột

namespace ScreenTranslator
{
    public partial class MainWindow : Window
    {
        // --- 1. CẤU HÌNH HOTKEY ---
        const int MOD_CONTROL = 0x0002;
        const int VK_SPACE = 0x20;
        const int HOTKEY_ID = 9000;
        const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Biến cờ để kiểm tra xem đã set vị trí lần đầu chưa
        private bool _hasSetPosition = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;
            RegisterHotKey(handle, HOTKEY_ID, MOD_CONTROL, VK_SPACE);

            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                HandleTranslation();
                handled = true;
            }
            return IntPtr.Zero;
        }

        // --- 2. XỬ LÝ KÉO THẢ CỬA SỔ ---
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Cho phép người dùng nhấn giữ chuột trái để kéo cửa sổ đi
            this.DragMove();
        }

        // --- 3. XỬ LÝ HIỂN THỊ ---
        private void HandleTranslation()
        {
            try
            {
                // Copy và lấy text
                System.Windows.Forms.SendKeys.SendWait("^c");
                Thread.Sleep(200);

                if (System.Windows.Clipboard.ContainsText())
                {
                    txtResult.Text = System.Windows.Clipboard.GetText();

                    // Logic hiển thị mới
                    ShowPopupMemory();
                }
                else
                {
                    txtResult.Text = "Không copy được nội dung!";
                    ShowPopupMemory();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void ShowPopupMemory()
        {
            // Hiện cửa sổ lên trước để tính toán kích thước
            this.Visibility = Visibility.Visible;
            this.UpdateLayout();

            // Kiểm tra: Nếu đây là lần đầu tiên mở app lên
            if (!_hasSetPosition)
            {
                // Tính toán vị trí GÓC DƯỚI BÊN TRÁI màn hình
                var screenHeight = System.Windows.SystemParameters.WorkArea.Height; // Chiều cao vùng làm việc (trừ thanh Taskbar)
                var windowHeight = this.ActualHeight;

                this.Left = 20; // Cách lề trái 20px
                this.Top = screenHeight - windowHeight - 20; // Cách đáy 20px

                // Đánh dấu là đã set xong.
                // Từ lần sau, code sẽ bỏ qua đoạn if này -> Giữ nguyên vị trí người dùng đã kéo
                _hasSetPosition = true;
            }

            // Luôn nổi lên trên cùng và focus
            this.Topmost = true;
            this.Activate();
        }

        protected override void OnClosed(EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, HOTKEY_ID);
            base.OnClosed(e);
        }
    }
}