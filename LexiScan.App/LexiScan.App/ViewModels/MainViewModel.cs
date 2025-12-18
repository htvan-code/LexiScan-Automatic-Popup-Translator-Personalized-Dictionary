using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using LexiScan.App.Commands;
using LexiScan.Core; // [QUAN TRỌNG] Thêm dòng này để dùng AppCoordinator

namespace LexiScan.App.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // [SỬA ĐỔI] Không khởi tạo DictionaryViewModel ngay tại đây nữa
        // Vì nó cần tham số coordinator, ta sẽ khởi tạo nó trong Constructor
        private readonly DictionaryViewModel _dictionaryVM;

        // Các ViewModel khác chưa cần Coordinator thì cứ giữ nguyên
        private readonly PersonalDictionaryViewModel _personalDictionaryVM = new PersonalDictionaryViewModel();
        private readonly HistoryViewModel _historyVM = new HistoryViewModel();
        private readonly TranslationViewModel _translationVM = new TranslationViewModel();
        private readonly SettingsViewModel _settingsVM = new SettingsViewModel();

        private BaseViewModel? _currentView;

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
        public ICommand LogoutCommand { get; }

        // [SỬA ĐỔI] Constructor phải nhận vào AppCoordinator
        public MainViewModel(AppCoordinator coordinator)
        {
            // [SỬA ĐỔI] Khởi tạo DictionaryViewModel và truyền coordinator vào
            _dictionaryVM = new DictionaryViewModel(coordinator);

            NavigateCommand = new RelayCommand(Navigate);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            // Mặc định hiển thị Trang chủ
            CurrentView = _dictionaryVM;
        }

        private void Navigate(object? parameter)
        {
            string? viewName = parameter as string;
            if (string.IsNullOrEmpty(viewName)) return;

            CurrentView = viewName switch
            {
                "Home" => _dictionaryVM,
                "Dictionary" => _personalDictionaryVM, // Lưu ý: Logic cũ của bạn map Dictionary -> Personal, bạn có thể chỉnh lại nếu muốn
                "PersonalDictionary" => _personalDictionaryVM,
                "History" => _historyVM,
                "Translation" => _translationVM,
                "Settings" => _settingsVM,
                _ => CurrentView
            };
        }

        private void ExecuteLogout(object? parameter)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Bước 1: Xóa Token
                // Lưu ý: Đảm bảo bạn đã có Properties.Settings trong project App
                LexiScan.App.Properties.Settings.Default.UserId = "";
                LexiScan.App.Properties.Settings.Default.Save();

                // Bước 2: Tự động khởi động lại ứng dụng
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