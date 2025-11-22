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


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SettingsViewModel()
        {
            
            _currentSettings = _settingsService.LoadSettings();

            
            SaveCommand = new RelayCommand(SaveSettings);
        }

        
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

        
        public ICommand SaveCommand { get; }

        private void SaveSettings(object parameter)
        {
            
            _settingsService.SaveSettings(_currentSettings);

            
            System.Windows.MessageBox.Show("Cài đặt đã được lưu thành công!");
        }
    }

}
