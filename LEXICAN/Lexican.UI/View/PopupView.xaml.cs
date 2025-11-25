// Project: LexiScan.UI
// File: Views/PopupView.xaml.cs

using System.Windows.Controls;
using LexiScan.UI.ViewModels; // Cần dùng ViewModel để khởi tạo DataContext
using System; // Cần thiết cho các lỗi khác, nên giữ

// BƯỚC 1: Bọc lớp trong Namespace CHÍNH XÁC
namespace LexiScan.UI.Views
{
    public partial class PopupView : UserControl
    {
        public PopupView()
        {
            // LỆNH NÀY SẼ GỌI PHƯƠNG THỨC ĐƯỢC SINH RA TỰ ĐỘNG
            InitializeComponent();

            // Thiết lập DataContext
            // Lưu ý: Đảm bảo PopupViewModel đã được đặt trong namespace LexiScan.UI.ViewModels
            this.DataContext = new PopupViewModel();
        }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }

        // BƯỚC 2: XÓA hoàn toàn phương thức InitializeComponent() sau
        /* private void InitializeComponent()
        {
            throw new NotImplementedException();
        }
        */
    }
}