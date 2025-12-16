using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
using System;
using System.ComponentModel;
using System.Text;
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

        }

        private void OnTranslationResultReceived(TranslationResult result)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (result.Status == ServiceStatus.Success)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine($"📖 {result.OriginalText}");

                    if (!string.IsNullOrEmpty(result.Phonetic))
                    {
                        sb.AppendLine($"/{result.Phonetic}/");
                    }
                    else
                    {
                    }

                    sb.AppendLine("-----------------------------");

                    sb.AppendLine($"✅ {result.TranslatedText}");
                    sb.AppendLine();
                    if (result.Meanings != null && result.Meanings.Count > 0)
                    {
                        foreach (var m in result.Meanings)
                        {
                            sb.AppendLine($"--- {m.PartOfSpeech} ---");

                            string joinedDefs = string.Join(", ", m.Definitions);
                            sb.AppendLine(joinedDefs);
                            sb.AppendLine(); 
                        }
                    }

                    System.Windows.MessageBox.Show(sb.ToString(), "Kết quả dịch");
                }
                else
                {
                    System.Windows.MessageBox.Show($"⚠️ Lỗi: {result.ErrorMessage}");
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