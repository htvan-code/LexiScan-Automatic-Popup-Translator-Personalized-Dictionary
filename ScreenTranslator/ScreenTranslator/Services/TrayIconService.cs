using System;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace ScreenTranslator
{
    public class TrayService
    {
        private WinForms.NotifyIcon _notifyIcon;
        private Window _mainWindow;

        public TrayService(Window window)
        {
            _mainWindow = window;
        }

        public void Initialize()
        {
            _notifyIcon = new WinForms.NotifyIcon();
            try
            {
                string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Components", "app_icon.ico");
                _notifyIcon.Icon = new Drawing.Icon(iconPath);
            }
            catch
            {
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Screen Translator (Click phải để xem Menu)";
            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

            _notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == WinForms.MouseButtons.Right)
                {
                    var trayMenu = System.Windows.Application.Current.FindResource("MainTrayMenu") as ContextMenu;

                    if (trayMenu != null)
                    {
                        AssignClickEvents(trayMenu);

                        trayMenu.IsOpen = true;
                        _mainWindow.Activate();
                    }
                }
            };
        }

        private void AssignClickEvents(ContextMenu menu)
        {
            foreach (var item in menu.Items)
            {
                if (item is MenuItem menuItem)
                {
                    menuItem.Click -= MenuOpen_Click;
                    menuItem.Click -= MenuExit_Click;
                    menuItem.Click -= MenuStartup_Click;

                    if (menuItem.Name == "MenuOpen") menuItem.Click += MenuOpen_Click;
                    if (menuItem.Name == "MenuExit") menuItem.Click += MenuExit_Click;
                    if (menuItem.Name == "MenuStartup")
                    {
                        menuItem.IsChecked = IsStartupEnabled();
                        menuItem.Click += MenuStartup_Click;
                    }
                }
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e) => ShowMainWindow();

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            _notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private void MenuStartup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item) SetStartup(item.IsChecked);
        }

        private void ShowMainWindow()
        {
            if (_mainWindow != null)
            {
                // 1. Nếu cửa sổ đang bị thu nhỏ, phải khôi phục lại kích thước bình thường
                if (_mainWindow.WindowState == WindowState.Minimized)
                {
                    _mainWindow.WindowState = WindowState.Normal;
                }

                // 2. Hiện cửa sổ
                _mainWindow.Visibility = Visibility.Visible;
                _mainWindow.Show();

                // 3. Đưa cửa sổ lên trên cùng (Active)
                _mainWindow.Activate();
                _mainWindow.Topmost = true;  // Mẹo: Bật Topmost
                _mainWindow.Topmost = false; // Tắt ngay để người dùng còn bấm sang cửa sổ khác được
                _mainWindow.Focus();
            }
        }

        private void SetStartup(bool enable)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    string path = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
                    if (enable) key.SetValue("ScreenTranslator", "\"" + path + "\"");
                    else key.DeleteValue("ScreenTranslator", false);
                }
            }
            catch { }
        }

        private bool IsStartupEnabled()
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                return key.GetValue("ScreenTranslator") != null;
            }
        }
    }
}