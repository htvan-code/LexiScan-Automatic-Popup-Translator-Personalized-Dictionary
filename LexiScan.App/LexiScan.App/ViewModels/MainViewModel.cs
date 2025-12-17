using System;
using System.Collections.Generic;
using System.Diagnostics; // [MỚI] Để dùng Process.Start (Khởi động lại app)
using System.Windows;       // [MỚI] Để dùng MessageBox và Application
using System.Windows.Input;
using LexiScan.App.Commands;

namespace LexiScan.App.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // Khởi tạo các ViewModel con (Singleton)
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
        public ICommand LogoutCommand { get; } // [MỚI] Khai báo lệnh Đăng xuất

        public MainViewModel()
        {
            NavigateCommand = new RelayCommand(Navigate);

            // [MỚI] Khởi tạo lệnh Đăng xuất
            LogoutCommand = new RelayCommand(ExecuteLogout);

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
                "Home" => _dictionaryVM,
                "Dictionary" => _personalDictionaryVM,
                "PersonalDictionary" => _personalDictionaryVM,
                "History" => _historyVM,
                "Translation" => _translationVM,
                "Settings" => _settingsVM,
                _ => CurrentView
            };
        }

        // [MỚI] Logic xử lý Đăng xuất
        private void ExecuteLogout(object? parameter)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Bước 1: Xóa Token đã lưu trong Settings
                LexiScan.App.Properties.Settings.Default.UserToken = "";
                LexiScan.App.Properties.Settings.Default.Save();

                // Bước 2: Tự động khởi động lại ứng dụng
                // Lệnh này sẽ lấy đường dẫn file .exe hiện tại và chạy lại nó
                string? appPath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(appPath))
                {
                    Process.Start(appPath);
                }

                // Bước 3: Tắt instance hiện tại
                Application.Current.Shutdown();
            }
        }
    }
}