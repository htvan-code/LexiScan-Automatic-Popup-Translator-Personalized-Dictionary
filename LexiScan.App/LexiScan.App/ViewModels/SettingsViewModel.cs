using LexiScan.App.Commands;
using LexiScan.App.Models;
using LexiScan.App.Models.LexiScan.App.Models;
using LexiScan.App.Services;
using System.Windows;
using System.Windows.Input;
using LexiScan.App; // [QUAN TRỌNG] Thêm dòng này để gọi App.ChangeTheme

namespace LexiScan.App.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly SettingsService _settingsService = new SettingsService();
        private Settings _currentSettings;

        public SettingsViewModel()
        {
            _currentSettings = _settingsService.LoadSettings();

            // [QUAN TRỌNG] Áp dụng theme ngay khi ViewModel được khởi tạo
            // Để đảm bảo app hiển thị đúng màu đã lưu trước đó
            App.ChangeTheme(_currentSettings.IsDarkModeEnabled);

            SaveCommand = new RelayCommand(SaveSettings);
            CancelCommand = new RelayCommand(_ => { /* Logic hủy nếu cần */ });
            ExportDataCommand = new RelayCommand(_ => { /* Logic export */ });
            ChangeHotkeyCommand = new RelayCommand(_ => { /* Logic đổi hotkey */ });
        }

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

        // [QUAN TRỌNG] Thuộc tính này sẽ được Bind vào Toggle/Checkbox bên View
        public bool IsDarkModeEnabled
        {
            get => _currentSettings.IsDarkModeEnabled;
            set
            {
                if (_currentSettings.IsDarkModeEnabled != value)
                {
                    // 1. Cập nhật giá trị vào Model
                    _currentSettings.IsDarkModeEnabled = value;

                    // 2. Thông báo cho giao diện biết dữ liệu đã thay đổi
                    OnPropertyChanged();

                    // 3. Gọi hàm đổi Theme ngay lập tức
                    App.ChangeTheme(value);
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand ChangeHotkeyCommand { get; }

        private void SaveSettings(object? _)
        {
            _settingsService.SaveSettings(_currentSettings);
            MessageBox.Show("Cài đặt đã được lưu thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}