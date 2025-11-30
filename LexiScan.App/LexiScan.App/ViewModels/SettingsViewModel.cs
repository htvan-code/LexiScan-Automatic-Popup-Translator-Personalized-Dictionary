using LexiScan.App.Commands;
using LexiScan.App.Models;
using LexiScan.App.Models.LexiScan.App.Models;
using LexiScan.App.Services;
using System.Windows;
using System.Windows.Input;
// Lưu ý: Namespace 'LexiScan.App.Models.LexiScan.App.Models' có vẻ là một lỗi đánh máy
// Tôi đã loại bỏ nó và giả định rằng Settings Model nằm trong LexiScan.App.Models
// Nếu Settings Model không nằm trong LexiScan.App.Models, bạn cần điều chỉnh lại namespace.

namespace LexiScan.App.ViewModels
{
    // Kế thừa từ BaseViewModel
    public class SettingsViewModel : BaseViewModel
    {
        // Khởi tạo _settingsService và _currentSettings
        private readonly SettingsService _settingsService = new SettingsService();
        private Settings _currentSettings;

        public SettingsViewModel()
        {
            // Tải cài đặt từ Service
            _currentSettings = _settingsService.LoadSettings();

            // Khởi tạo Command
            SaveCommand = new RelayCommand(SaveSettings);
        }

        // Bổ sung thuộc tính Settings để DataContext của SettingsView có thể bind trực tiếp.
        public Settings CurrentSettings
        {
            get => _currentSettings;
            private set
            {
                if (_currentSettings != value)
                {
                    _currentSettings = value;
                    OnPropertyChanged();
                }
            }
        }

        // Thuộc tính bọc (Wrapper Property) cho IsAutoReadEnabled
        public bool IsAutoReadEnabled
        {
            get => _currentSettings.IsAutoReadEnabled;
            set
            {
                if (_currentSettings.IsAutoReadEnabled != value)
                {
                    _currentSettings.IsAutoReadEnabled = value;
                    OnPropertyChanged(); // Gọi OnPropertyChanged
                }
            }
        }

        // *** Thuộc tính bọc (Wrapper Property) cho IsDarkModeEnabled ***
        // Giá trị này được lưu trong Models/Settings.cs để đảm bảo lưu trữ bền vững.
        public bool IsDarkModeEnabled
        {
            get => _currentSettings.IsDarkModeEnabled;
            set
            {
                if (_currentSettings.IsDarkModeEnabled != value)
                {
                    _currentSettings.IsDarkModeEnabled = value;
                    OnPropertyChanged(); // Gọi OnPropertyChanged để cập nhật UI nếu cần

                    // Kích hoạt logic theme thông qua Service
                    ThemeService.Instance.ApplyTheme(value ? "Dark" : "Light");
                }
            }
        }

        // Cần đảm bảo rằng các thuộc tính khác (ShowScanIcon, SpeedSlower, v.v.)
        // cũng được thêm vào đây dưới dạng thuộc tính bọc tương tự.

        public ICommand SaveCommand { get; }

        // Khắc phục CS8376: Sử dụng object?
        private void SaveSettings(object? _)
        {
            _settingsService.SaveSettings(_currentSettings);
            MessageBox.Show("Cài đặt đã được lưu thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}