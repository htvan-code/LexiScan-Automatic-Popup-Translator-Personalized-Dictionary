using LexiScan.App.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LexiScan.App.Models; 
using LexiScan.App.Services;
using LexiScan.App.Models.LexiScan.App.Models;
namespace LexiScan.App.ViewModels
{

    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService = new SettingsService();
        private Settings _currentSettings;

        // Triển khai INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SettingsViewModel()
        {
            // Tải cài đặt khi ViewModel được tạo
            _currentSettings = _settingsService.LoadSettings();

            // Khởi tạo Command
            SaveCommand = new RelayCommand(SaveSettings);
        }

        // Properties dùng cho Data Binding (sử dụng thuộc tính hiện tại)
        public bool IsAutoReadEnabled
        {
            get => _currentSettings.IsAutoReadEnabled;
            set
            {
                if (_currentSettings.IsAutoReadEnabled != value)
                {
                    _currentSettings.IsAutoReadEnabled = value;
                    OnPropertyChanged(nameof(IsAutoReadEnabled));
                }
            }
        }

        // Command cho nút "Lưu Cài Đặt"
        public ICommand SaveCommand { get; }

        private void SaveSettings(object parameter)
        {
            // 1. Lưu đối tượng hiện tại vào file JSON
            _settingsService.SaveSettings(_currentSettings);

            // 2. Tùy chọn: Thông báo cho người dùng hoặc các service khác
            System.Windows.MessageBox.Show("Cài đặt đã được lưu thành công!");
        }
    }

}
