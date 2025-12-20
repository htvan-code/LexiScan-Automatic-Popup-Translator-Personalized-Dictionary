using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using LexiScan.App.Commands;
using LexiScan.Core;
using LexiScan.Core.Utils; // [QUAN TRỌNG] Để dùng AppCoordinator và GlobalEvents

namespace LexiScan.App.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // Khởi tạo DictionaryViewModel và truyền coordinator vào (sẽ làm trong Constructor)
        private readonly DictionaryViewModel _dictionaryVM;

        // Các ViewModel khác
        private readonly PersonalDictionaryViewModel _personalDictionaryVM = new PersonalDictionaryViewModel();
        private readonly HistoryViewModel _historyVM = new HistoryViewModel();
        private readonly TranslationViewModel _translationVM;
        private readonly SettingsViewModel _settingsVM = new SettingsViewModel();

        private BaseViewModel? _currentView;
        private string _selectedMenu = "Home"; // [MỚI] Biến để theo dõi menu đang chọn

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

        // [MỚI] Property để Binding vào IsChecked của Menu (để menu tự sáng khi chuyển trang)
        public string SelectedMenu
        {
            get => _selectedMenu;
            set { _selectedMenu = value; OnPropertyChanged(); }
        }

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }

        // Constructor nhận vào AppCoordinator
        public MainViewModel(AppCoordinator coordinator)
        {
            // Khởi tạo DictionaryViewModel với coordinator
            _dictionaryVM = new DictionaryViewModel(coordinator);

            _translationVM = new TranslationViewModel(coordinator);

            NavigateCommand = new RelayCommand(Navigate);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            // Mặc định hiển thị Trang chủ
            CurrentView = _dictionaryVM;
            SelectedMenu = "Home";

            // [MỚI] Đăng ký lắng nghe sự kiện mở Settings từ GlobalEvents (do Popup gọi)
            GlobalEvents.OnRequestOpenSettings += HandleOpenSettingsRequest;
        }

        // [MỚI] Hàm xử lý khi nhận được yêu cầu mở Settings từ Popup
        private void HandleOpenSettingsRequest()
        {
            // Dùng Dispatcher để ép buộc chạy trên luồng giao diện chính
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // 1. Chuyển View
                CurrentView = _settingsVM;
                SelectedMenu = "Settings";

                // 2. Hiện cửa sổ chính lên
                var mainWindow = System.Windows.Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    if (mainWindow.WindowState == WindowState.Minimized)
                    {
                        mainWindow.WindowState = WindowState.Normal;
                    }
                    mainWindow.Show();
                    mainWindow.Activate();
                    mainWindow.Focus();
                }
            });
        }
        private void Navigate(object? parameter)
        {
            string? viewName = parameter as string;
            if (string.IsNullOrEmpty(viewName)) return;

            // [MỚI] Cập nhật SelectedMenu khi người dùng bấm nút điều hướng
            SelectedMenu = viewName;

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

        private void ExecuteLogout(object? parameter)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Bước 1: Xóa Token/UserId (Dùng UserId theo code của bạn)
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