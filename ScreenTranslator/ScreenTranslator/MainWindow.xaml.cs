using System;
using System.ComponentModel; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Drawing = System.Drawing;
using WinForms = System.Windows.Forms;

namespace ScreenTranslator
{
    public partial class MainWindow : Window
    {
        private ClipboardHookService _hookService;
        private bool _hasSetPosition = false;

        private int _currentMod = ClipboardHookService.MOD_CONTROL;
        private int _currentKey = 0x20;

        private WinForms.NotifyIcon _notifyIcon;
        private bool _isRealExit = false;

        public MainWindow()
        {
            InitializeComponent();

            _hookService = new ClipboardHookService();
            _hookService.OnTextCaptured += ShowResult;
            _hookService.OnError += ShowError;
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
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_isRealExit)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                if (_hookService != null) _hookService.Unregister();
            }

            base.OnClosing(e);
        }

        private void ExitApplication()
        {
            _isRealExit = true; 
            _notifyIcon.Dispose(); 
            System.Windows.Application.Current.Shutdown(); 
        }

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
            _notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == WinForms.MouseButtons.Right)
                {
                    var trayMenu = this.Resources["TrayMenu"] as ContextMenu;
                    if (trayMenu != null)
                    {
                        foreach (var item in trayMenu.Items)
                        {
                            if (item is MenuItem menuItem && menuItem.Name == "MenuStartup")
                            {
                                menuItem.IsChecked = IsStartupEnabled();
                                break;
                            }
                        }
                        trayMenu.IsOpen = true;
                        this.Activate();
                    }
                }
            };
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            ShowPopupMemory();
        }

        private void MenuStartup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                SetStartup(item.IsChecked);
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }
    }
}