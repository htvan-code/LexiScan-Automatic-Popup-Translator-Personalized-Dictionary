using System.Windows;
// Đảm bảo không có các using thừa từ ViewModel ở đây

namespace LexiScan.App
{
    // Lớp này phải là "partial class MainWindow"
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // KHÔNG THÊM BẤT KỲ LOGIC NAVIGATION NÀO Ở ĐÂY.
            // Logic navigation được xử lý bởi MainViewModel qua DataContext.
        }
    }
}