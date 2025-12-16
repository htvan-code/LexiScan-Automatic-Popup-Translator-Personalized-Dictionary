using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Threading.Tasks; 
using LexiScan.Core;

namespace ScreenTranslator
{
    public partial class MainWindow : Window
    {
        private ClipboardHookService _hookService;
        private TrayService _trayService;

        private int _currentMod = ClipboardHookService.MOD_CONTROL;
        private int _currentKey = 0x20;
        private bool _hasSetPosition = false;

        public MainWindow()
        {
            InitializeComponent();

            _hookService = new ClipboardHookService();
            _hookService.OnTextCaptured += ShowResult;
            _hookService.OnError += (msg) => { txtResult.Text = msg; ShowPopupMemory(); };

            _trayService = new TrayService(this);

            _trayService.Initialize();
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

            if (!string.IsNullOrWhiteSpace(text))
            {
                Task.Run(() =>
                {
                    AppCoordinator.Instance.ProcessCapturedText(text);
                });
            }
        }

        public void ShowPopupMemory()
        {
            this.Visibility = Visibility.Visible;
            this.Show();

            if (!_hasSetPosition)
            {
                var screenWidth = SystemParameters.WorkArea.Width;
                var screenHeight = SystemParameters.WorkArea.Height;
                this.Left = screenWidth - this.Width - 20;
                this.Top = screenHeight - this.Height - 20;
                _hasSetPosition = true;
            }

            this.Topmost = true;
            this.Activate();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) this.DragMove();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            base.OnClosing(e);
        }
    }
}