using System;
using System.ComponentModel; // Cần cái này cho CancelEventArgs
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

// --- 1. QUAN TRỌNG: Đặt tên riêng để tránh xung đột giữa WPF và WinForms ---
using WinForms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace ScreenTranslator
{
    public partial class MainWindow : Window
    {
        // Khai báo Service (Cần câu)
        private ClipboardHookService _hookService;
        private bool _hasSetPosition = false;

        private int _currentMod = ClipboardHookService.MOD_CONTROL;
        private int _currentKey = 0x20;

        // --- 2. QUAN TRỌNG: Khai báo biến cho System Tray ---
        // (Lúc nãy bạn bị đỏ là do thiếu 2 dòng này)
        private WinForms.NotifyIcon _notifyIcon;
        private bool _isRealExit = false;

        public MainWindow()
        {
            InitializeComponent();

            _hookService = new ClipboardHookService();
            _hookService.OnTextCaptured += ShowResult;
            _hookService.OnError += ShowError;

            // Gọi hàm tạo Icon ở đây
            InitSystemTray();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;

            _hookService.Register(handle, _currentMod, _currentKey);

            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
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

        // --- XỬ LÝ ĐÓNG CỬA SỔ & THOÁT ---

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Nếu chưa bấm nút "Thoát" dưới tray icon thì chỉ ẩn đi
            if (!_isRealExit)
            {
                e.Cancel = true;
                this.Hide();
            }
            // Nhớ gọi Unregister khi đóng thật
            else
            {
                if (_hookService != null) _hookService.Unregister();
            }

            base.OnClosing(e);
        }

        private void ExitApplication()
        {
            _isRealExit = true; // Bật cờ cho phép thoát thật
            _notifyIcon.Dispose(); // Xóa icon
            System.Windows.Application.Current.Shutdown(); // Tắt app
        }

        // --- CÁC HÀM STARTUP & SYSTEM TRAY ---

        private void SetStartup(bool enable)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (enable)
                    {
                        string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        path = path.Replace(".dll", ".exe");
                        key.SetValue("ScreenTranslator", "\"" + path + "\"");
                    }
                    else
                    {
                        key.DeleteValue("ScreenTranslator", false);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi cài đặt Startup: " + ex.Message);
            }
        }

        private bool IsStartupEnabled()
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                return key.GetValue("ScreenTranslator") != null;
            }
        }

        private void InitSystemTray()
        {
            _notifyIcon = new WinForms.NotifyIcon();

            try
            {
                string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_icon.ico");
                _notifyIcon.Icon = new Drawing.Icon(iconPath);
            }
            catch
            {
                _notifyIcon.Icon = Drawing.SystemIcons.Shield;
            }

            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Screen Translator (Đang chạy ngầm)";

            _notifyIcon.DoubleClick += (s, e) => ShowPopupMemory();

            // Menu chuột phải
            WinForms.ContextMenuStrip menu = new WinForms.ContextMenuStrip();

            var mainItem = new WinForms.ToolStripMenuItem("Mở giao diện chính");
            mainItem.Font = new Drawing.Font(mainItem.Font, Drawing.FontStyle.Bold);
            mainItem.Click += (s, e) => ShowPopupMemory();
            menu.Items.Add(mainItem);

            menu.Items.Add(new WinForms.ToolStripSeparator());

            var startupItem = new WinForms.ToolStripMenuItem("Khởi động cùng Windows");
            startupItem.Checked = IsStartupEnabled();
            startupItem.Click += (s, e) =>
            {
                bool newState = !startupItem.Checked;
                SetStartup(newState);
                startupItem.Checked = newState;
            };
            menu.Items.Add(startupItem);

            menu.Items.Add(new WinForms.ToolStripSeparator());

            var closeItem = new WinForms.ToolStripMenuItem("Thoát");
            closeItem.Click += (s, e) => ExitApplication();
            menu.Items.Add(closeItem);

            _notifyIcon.ContextMenuStrip = menu;
        }
    }
}