using System.Windows.Input;
using LexiScan.App.Commands;
using System;
using System.Collections.Generic;
using LexiScan.App.ViewModels;
namespace LexiScan.App.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // Khởi tạo các ViewModel con (Singleton)
        // MainViewModel sẽ quản lý các instance này
        private readonly DictionaryViewModel _dictionaryVM = new DictionaryViewModel();
        private readonly PersonalDictionaryViewModel _personalDictionaryVM = new PersonalDictionaryViewModel();
        private readonly HistoryViewModel _historyVM = new HistoryViewModel();
        private readonly TranslationViewModel _translationVM = new TranslationViewModel();
        private readonly SettingsViewModel _settingsVM = new SettingsViewModel();

        private BaseViewModel? _currentView;

        // Thuộc tính bind với ContentControl trong MainWindow
        public BaseViewModel? CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NavigateCommand { get; }

        public MainViewModel()
        {
            NavigateCommand = new RelayCommand(Navigate);

            // Mặc định hiển thị Trang chủ (DictionaryView) khi mở app
            CurrentView = _dictionaryVM;
        }

        private void Navigate(object? parameter)
        {
            string? viewName = parameter as string;
            if (string.IsNullOrEmpty(viewName)) return;

            // Logic chuyển đổi View dựa trên CommandParameter từ RadioButton
            CurrentView = viewName switch
            {
                "Home" => _dictionaryVM,                 // Trang chủ
                "Dictionary" => _personalDictionaryVM,   // Từ điển cá nhân (Mapping từ param 'Dictionary')
                "PersonalDictionary" => _personalDictionaryVM, // Mapping dự phòng
                "History" => _historyVM,                 // Lịch sử
                "Translation" => _translationVM,         // Dịch thuật
                "Settings" => _settingsVM,               // Cài đặt
                _ => CurrentView
            };
        }
    }


}