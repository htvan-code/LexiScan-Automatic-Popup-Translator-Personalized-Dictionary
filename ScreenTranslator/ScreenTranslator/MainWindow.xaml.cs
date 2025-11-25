using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms; // Dùng cho SendKeys & Cursor
using System.Windows.Interop; // Dùng để Hook tin nhắn Windows

namespace ScreenTranslator
{
    public partial class MainWindow : Window
    {
        // --- 1. KHAI BÁO DLL & HẰNG SỐ (Giữ nguyên từ tuần trước) ---
        const int MOD_CONTROL = 0x0002;
        const int WM_HOTKEY = 0x0312;
        const int HOTKEY_ID = 9000;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public MainWindow()
        {
            InitializeComponent();
        }

        // --- 2. KHỞI TẠO HOOK KHI CỬA SỔ ĐƯỢC TẠO ---
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Lấy Handle (ID) của cửa sổ này
            IntPtr handle = new WindowInteropHelper(this).Handle;

            // Đăng ký Hotkey: Ctrl + Space (0x20)
            RegisterHotKey(handle, HOTKEY_ID, MOD_CONTROL, 0x20);

            // Gắn cái "tai" (Hook) vào cửa sổ để nghe tin nhắn từ Windows
            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);
        }

        // --- 3. HÀM LẮNG NGHE (Thay cho vòng lặp GetMessage cũ) ---
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                // Khi bấm Hotkey -> Gọi hàm xử lý
                HandleTranslation();
                handled = true;
            }
            return IntPtr.Zero;
        }

        // --- 4. LOGIC XỬ LÝ CHÍNH ---
        private void HandleTranslation()
        {
            try
            {
                // A. Giả lập Copy
                System.Windows.Clipboard.Clear();
                SendKeys.SendWait("^c");
                Thread.Sleep(150);

                // B. Lấy text
                if (System.Windows.Clipboard.ContainsText())
                {
                    string text = System.Windows.Clipboard.GetText();

                    // C. Hiển thị lên giao diện
                    txtResult.Text = text; // Gán text vào ô trên màn hình
                    ShowPopupAtMouse();    // Hiện cửa sổ ngay tại chuột
                }
            }
            catch (Exception ex)
            {
                txtResult.Text = "Lỗi: " + ex.Message;
                ShowPopupAtMouse();
            }
        }

        // --- 5. HÀM HIỆN CỬA SỔ TẠI CHUỘT ---
        private void ShowPopupAtMouse()
        {
            // Lấy tọa độ chuột hiện tại
            var mouseX = System.Windows.Forms.Cursor.Position.X;
            var mouseY = System.Windows.Forms.Cursor.Position.Y;

            // Đặt vị trí cửa sổ (Cách chuột một chút cho đỡ che)
            this.Left = mouseX + 15;
            this.Top = mouseY + 15;

            // Hiện cửa sổ lên
            this.Visibility = Visibility.Visible;
            this.Activate(); // Focus vào cửa sổ
        }

        // --- 6. DỌN DẸP KHI TẮT APP ---
        protected override void OnClosed(EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, HOTKEY_ID);
            base.OnClosed(e);
        }
    }
}