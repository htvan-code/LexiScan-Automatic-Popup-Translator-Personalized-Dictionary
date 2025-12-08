// LexiScan.App.ViewModels/SettingsViewModel.cs
using LexiScan.App.Commands;
using LexiScan.App.Models;
using LexiScan.App.Models.LexiScan.App.Models;
using LexiScan.App.Services;
using System.Windows;
using System.Windows.Input;

namespace LexiScan.App.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly SettingsService _settingsService = new SettingsService();
        private Settings _currentSettings;

        // Đã xóa: private double _temporaryThemeSliderValue; 

        public SettingsViewModel()
        {
            _currentSettings = _settingsService.LoadSettings();

            // Đã xóa: logic khởi tạo _temporaryThemeSliderValue

            SaveCommand = new RelayCommand(SaveSettings);
            CancelCommand = new RelayCommand(_ => { /* Logic hủy */ });
            ExportDataCommand = new RelayCommand(_ => { /* Logic export */ });
            ChangeHotkeyCommand = new RelayCommand(_ => { /* Logic đổi hotkey */ });
        }

        // Đã xóa: public double TemporaryThemeSliderValue { get; set; }

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

        // ... (Giữ nguyên các thuộc tính wrapper khác: IsAutoReadEnabled, v.v.)

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand ChangeHotkeyCommand { get; }


        private void SaveSettings(object? _)
        {
            // Không cần cập nhật giá trị IsDarkModeEnabled từ Slider nữa,
            // chỉ cần lưu trạng thái hiện tại (nếu nó được bind với Checkbox/Toggle)
            _settingsService.SaveSettings(_currentSettings);
            MessageBox.Show("Cài đặt đã được lưu thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}