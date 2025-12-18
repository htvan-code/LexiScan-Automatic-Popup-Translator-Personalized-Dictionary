using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using LexiScanUI.ViewModels;
using LexiScan.Core.Models;

namespace LexiScanUI.View
{
    public partial class PopupView : Window
    {
        public PopupViewModel ViewModel { get; }

        public PopupView()
        {
            InitializeComponent();

            // GÁN VIEWMODEL
            ViewModel = new PopupViewModel();
            DataContext = ViewModel;

            // ===== PHỤC HỒI VỊ TRÍ CŨ CỦA POPUP =====
            double x = Properties.Settings.Default.PopupX;
            double y = Properties.Settings.Default.PopupY;

            if (x > 0 && y > 0)
            {
                this.Left = x;
                this.Top = y;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        // =========================================
        //   LƯU VỊ TRÍ POPUP KHI ĐÓNG
        // =========================================
        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.PopupX = this.Left;
            Properties.Settings.Default.PopupY = this.Top;
            Properties.Settings.Default.Save();

            base.OnClosing(e);
        }

        // =========================================
        //   KÉO POPUP BẰNG CHUỘT (DragMove)
        // =========================================
        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        // =========================================
        //   NHẬN KẾT QUẢ TỪ MAINWINDOW
        // =========================================
        public void ShowResult(TranslationResult result)
        {
            if (result == null) return;

            ViewModel.LoadTranslationData(result);

            Show();
            Activate();
        }

        // =========================================
        //   NÚT ĐÓNG POPUP
        // =========================================
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Hide(); // không destroy
        }
    }
}
