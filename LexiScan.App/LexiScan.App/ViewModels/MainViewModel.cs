using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using LexiScan.App.Commands;
using LexiScan.Core;
using LexiScan.Core.Utils;

namespace LexiScan.App.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DictionaryViewModel _dictionaryVM;

        private readonly PersonalDictionaryViewModel _personalDictionaryVM;

        private readonly HistoryViewModel _historyVM;
        private readonly TranslationViewModel _translationVM;
        private readonly SettingsViewModel _settingsVM = new SettingsViewModel();

        private BaseViewModel? _currentView;
        private string _selectedMenu = "Home";

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

        public string SelectedMenu
        {
            get => _selectedMenu;
            set { _selectedMenu = value; OnPropertyChanged(); }
        }

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel(AppCoordinator coordinator)
        {
            _dictionaryVM = new DictionaryViewModel(coordinator);
            _translationVM = new TranslationViewModel(coordinator);
            _historyVM = new HistoryViewModel(coordinator);

            _personalDictionaryVM = new PersonalDictionaryViewModel(coordinator);

            NavigateCommand = new RelayCommand(Navigate);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            CurrentView = _dictionaryVM;
            SelectedMenu = "Home";

            GlobalEvents.OnRequestOpenSettings += HandleOpenSettingsRequest;
        }

        private void HandleOpenSettingsRequest()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentView = _settingsVM;
                SelectedMenu = "Settings";

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

            SelectedMenu = viewName;

            // [LOGIC TẢI LẠI DỮ LIỆU]
            if (viewName == "History")
            {
                _historyVM.LoadFirebaseHistory();
            }
            else if (viewName == "PersonalDictionary" || viewName == "Dictionary")
            {
                _personalDictionaryVM.LoadData();
            }

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
                LexiScan.App.Properties.Settings.Default.UserId = "";
                LexiScan.App.Properties.Settings.Default.Save();

                string? appPath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(appPath))
                {
                    Process.Start(appPath);
                }

                Application.Current.Shutdown();
            }
        }
    }
}