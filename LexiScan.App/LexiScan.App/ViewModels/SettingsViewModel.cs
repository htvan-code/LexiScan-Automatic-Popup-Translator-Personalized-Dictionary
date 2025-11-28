// File: ViewModels/SettingsViewModel.cs

using LexiScan.App.Commands;
using LexiScan.App.Models; // Đảm bảo namespace này khớp với nơi Settings được định nghĩa
using LexiScan.App.Models.LexiScan.App.Models;
using LexiScan.App.Services;
using System.Windows;
using System.Windows.Input;

namespace LexiScan.App.ViewModels
{
    // Kế thừa từ BaseViewModel
    public class SettingsViewModel : BaseViewModel
    {
        // Khởi tạo _settingsService và _currentSettings
        private readonly SettingsService _settingsService = new SettingsService();
        private Settings _currentSettings; // CS8618 đã được giải quyết bằng cách khởi tạo trong constructor

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

        public ICommand SaveCommand { get; }

        // Khắc phục CS8376: Sử dụng object?
        private void SaveSettings(object? _)
        {
            _settingsService.SaveSettings(_currentSettings);
            MessageBox.Show("Cài đặt đã được lưu thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
// **ĐÃ XÓA** định nghĩa giả định của Settings và SettingsService khỏi đây.