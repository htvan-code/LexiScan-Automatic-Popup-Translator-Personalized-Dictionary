using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // <-- Quan trọng: Để nhận diện SolidColorBrush

namespace LexiScanUI.View
{
    public partial class PopupView : UserControl
    {
        private bool _isDarkMode = false;
        private readonly BrushConverter _brushConverter = new BrushConverter();

        public PopupView()
        {
            InitializeComponent();
        }

        // Hàm hỗ trợ lấy màu nhanh từ mã Hex (Giúp code gọn hơn, không bị lỗi ColorConverter)
        private SolidColorBrush GetBrush(string hexColor)
        {
            return (SolidColorBrush)_brushConverter.ConvertFromString(hexColor);
        }

        private void DarkMode_Click(object sender, RoutedEventArgs e)
        {
            _isDarkMode = !_isDarkMode;

            if (_isDarkMode)
            {
                // --- CHẾ ĐỘ DARK MODE (BLACK PINK) ---
                this.Resources["AppBackground"] = GetBrush("#121212");       // Nền đen
                this.Resources["AppBorder"] = GetBrush("#333333");       // Viền xám
                this.Resources["PrimaryText"] = GetBrush("#FFE4E1");       // Chữ hồng phấn
                this.Resources["SecondaryText"] = GetBrush("#B08891");       // Chữ phụ
                this.Resources["AccentColor"] = GetBrush("#FF69B4");       // Hồng đậm (Hot Pink)
                this.Resources["IconColor"] = Brushes.WhiteSmoke;        // Icon trắng
                this.Resources["HoverBackground"] = GetBrush("#2D2D2D");
                this.Resources["SelectedBackground"] = GetBrush("#2A1A22");
            }
            else
            {
                // --- CHẾ ĐỘ LIGHT MODE (MẶC ĐỊNH) ---
                this.Resources["AppBackground"] = Brushes.White;
                this.Resources["AppBorder"] = GetBrush("#E0E0E0");
                this.Resources["PrimaryText"] = GetBrush("#333333");
                this.Resources["SecondaryText"] = GetBrush("#777777");
                this.Resources["AccentColor"] = GetBrush("#2D7FF9");       // Xanh dương
                this.Resources["IconColor"] = GetBrush("#666666");
                this.Resources["HoverBackground"] = GetBrush("#F5F5F5");
                this.Resources["SelectedBackground"] = GetBrush("#E3F2FD");
            }
        }

        // --- CÁC HÀM XỬ LÝ NÚT KHÁC (Kết nối với ViewModel) ---

        // Helper để lấy ViewModel
        private LexiScanUI.ViewModels.PopupViewModel? ViewModel => DataContext as LexiScanUI.ViewModels.PopupViewModel;

        private void Pin_Click(object sender, RoutedEventArgs e)
        {
            // Gọi lệnh Pin trong ViewModel
            if (ViewModel != null && ViewModel.PinCommand.CanExecute(null))
            {
                ViewModel.PinCommand.Execute(null);
            }
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null && ViewModel.SettingsCommand.CanExecute(null))
            {
                ViewModel.SettingsCommand.Execute(null);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // Đóng cửa sổ popup
            Window.GetWindow(this)?.Close();
        }
    }
}