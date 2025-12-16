using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace ScreenTranslator
{
    public partial class MainWindow : Window
    {
        private ClipboardHookService _hookService;
        private TrayService _trayService;
        private AppCoordinator _coordinator;

        private int _currentMod = ClipboardHookService.MOD_CONTROL;
        private int _currentKey = 0x20;
        private bool _hasSetPosition = false;

        public MainWindow()
        {
            InitializeComponent();

            var transService = new TranslationService();
            _coordinator = new AppCoordinator(transService);

            _coordinator.TranslationCompleted += OnTranslationResultReceived;

            _hookService = new ClipboardHookService();
            _hookService.OnTextCaptured += SendTextToCoordinator;

            _hookService.OnError += (errorMessage) => { System.Windows.MessageBox.Show(errorMessage); };

            _trayService = new TrayService(this);
            _trayService.Initialize();

            //this.Visibility = Visibility.Hidden;
        }

        private void OnTranslationResultReceived(TranslationResult result)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (result.Status == ServiceStatus.Success)
                {
                    System.Windows.MessageBox.Show($"📢 KẾT QUẢ TỪ P2 TRẢ VỀ:\n\n" +
                                    $"Gốc: {result.OriginalText}\n" +
                                    $"----------------------\n" +
                                    $"Dịch: {result.TranslatedText}",
                                    "Demo Hiển Thị (Giả lập P3)");
                }
                else
                {
                    System.Windows.MessageBox.Show($"⚠️ LỖI XỬ LÝ:\n{result.ErrorMessage}", "P2 Báo Lỗi");
                }
            });
        }

        private async void SendTextToCoordinator(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                await _coordinator.HandleClipboardTextAsync(text);
            }
        }

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
                System.Windows.MessageBox.Show("Lỗi đăng ký Hook: " + ex.Message);
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            _hookService.ProcessWindowMessage(msg, wParam.ToInt32());
            return IntPtr.Zero;
        }

        public void ShowMainWindow()
        {
            this.Visibility = Visibility.Visible;
            this.Show();
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
        }
    }
}