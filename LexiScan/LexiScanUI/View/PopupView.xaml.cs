using System.Windows;
using LexiScanUI.ViewModels;
using LexiScan.Core.Models;

namespace LexiScanUI.View
{
    public partial class PopupView : Window
    {
        public PopupView()
        {
            InitializeComponent();
            // Khởi tạo ViewModel và gán vào DataContext
            this.DataContext = new PopupViewModel();
        }

        // Helper lấy ViewModel nhanh
        private PopupViewModel? ViewModel => DataContext as PopupViewModel;

        // Hàm nhận kết quả dịch từ MainWindow
        public void ShowResult(TranslationResult result)
        {
            if (ViewModel != null)
            {
                ViewModel.LoadTranslationData(result);
            }

            this.Show();
            this.Activate(); 
        }

        // ========================
        // CLOSE POPUP
        // ========================
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}