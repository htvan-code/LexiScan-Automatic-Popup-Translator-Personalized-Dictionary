using LexiScan.App.Commands;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LexiScan.Core.Models;
using LexiScan.Core.Services;

namespace LexiScan.App.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly SettingsService _settingsService = new SettingsService();
        private Settings _originalSettings;
        private Settings _currentSettings;
        private bool _hasUnsavedChanges;

        private bool _isChangingHotkey;
        private string _hotkeyButtonText = "Thiết Lập";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ChangeHotkeyCommand { get; }
        public ICommand ExportDataCommand { get; }

        public SettingsViewModel()
        {
            CurrentSettings = _settingsService.LoadSettings();

            ApplyTheme(CurrentSettings.IsDarkModeEnabled);

            SaveCommand = new RelayCommand(SaveSettings);
            CancelCommand = new RelayCommand(CancelChanges);

            ChangeHotkeyCommand = new RelayCommand((o) => {
                IsChangingHotkey = true;
                HotkeyButtonText = "Đang bấm phím...";
            });
            ExportDataCommand = new RelayCommand(_ => { });
        }

        public Settings CurrentSettings
        {
            get => _currentSettings;
            set
            {
                if (_currentSettings != null) _currentSettings.PropertyChanged -= OnSettingsChanged;
                _currentSettings = value;
                if (_currentSettings != null)
                {
                    _originalSettings = (Settings)_currentSettings.Clone();
                    _currentSettings.PropertyChanged += OnSettingsChanged;
                }
                OnPropertyChanged();
                CheckIfDirty();
            }
        }

        public bool IsDarkModeEnabled
        {
            get => _currentSettings.IsDarkModeEnabled;
            set
            {
                if (_currentSettings.IsDarkModeEnabled != value)
                {
                    _currentSettings.IsDarkModeEnabled = value;
                    OnPropertyChanged();
                    ApplyTheme(value);
                }
            }
        }

        public bool HasUnsavedChanges { get => _hasUnsavedChanges; set { _hasUnsavedChanges = value; OnPropertyChanged(); } }

        public bool IsChangingHotkey { get => _isChangingHotkey; set { _isChangingHotkey = value; OnPropertyChanged(); } }
        public string HotkeyButtonText { get => _hotkeyButtonText; set { _hotkeyButtonText = value; OnPropertyChanged(); } }

        public void UpdateHotkey(string newHotkey)
        {
            if (IsChangingHotkey)
            {
                CurrentSettings.Hotkey = newHotkey;
                OnPropertyChanged(nameof(CurrentSettings));
                IsChangingHotkey = false;
                HotkeyButtonText = "Thiết Lập";
                CheckIfDirty();
            }
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.IsDarkModeEnabled))
            {
                OnPropertyChanged(nameof(IsDarkModeEnabled));
                ApplyTheme(_currentSettings.IsDarkModeEnabled);
            }
            CheckIfDirty();
        }

        private void CheckIfDirty()
        {
            if (_currentSettings == null || _originalSettings == null) return;
            HasUnsavedChanges = true;
        }

        private void SaveSettings(object parameter)
        {
            _settingsService.SaveSettings(_currentSettings);
            _originalSettings = (Settings)_currentSettings.Clone(); 
            HasUnsavedChanges = false; 
            MessageBox.Show("Đã lưu cài đặt!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelChanges(object parameter)
        {
            var backup = _originalSettings;
            _currentSettings.PropertyChanged -= OnSettingsChanged;

            _currentSettings.Hotkey = backup.Hotkey;
            _currentSettings.IsAutoReadEnabled = backup.IsAutoReadEnabled;
            _currentSettings.Speed = backup.Speed;
            _currentSettings.Voice = backup.Voice;
            _currentSettings.IsDarkModeEnabled = backup.IsDarkModeEnabled;
            _currentSettings.AutoPronounceOnLookup = backup.AutoPronounceOnLookup;
            _currentSettings.AutoPronounceOnTranslate = backup.AutoPronounceOnTranslate;

            _currentSettings.PropertyChanged += OnSettingsChanged;

            ApplyTheme(_currentSettings.IsDarkModeEnabled);
            OnPropertyChanged(nameof(IsDarkModeEnabled));
            OnPropertyChanged(nameof(CurrentSettings)); // Refresh UI

            HasUnsavedChanges = false;
        }
        private void ApplyTheme(bool isDark)
        {
            var app = Application.Current;
            if (app == null) return;

            string assemblyName = "LexiScan";
            string themePath = isDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
            string newUriString = $"pack://application:,,,/{assemblyName};component/{themePath}";

            try
            {
                ResourceDictionary? existingThemeDict = null;
                foreach (var dict in app.Resources.MergedDictionaries)
                {
                    if (dict.Source != null &&
                       (dict.Source.OriginalString.Contains("Themes/LightTheme.xaml") ||
                        dict.Source.OriginalString.Contains("Themes/DarkTheme.xaml")))
                    {
                        existingThemeDict = dict;
                        break;
                    }
                }

                var newThemeDict = new ResourceDictionary { Source = new Uri(newUriString, UriKind.Absolute) };

                if (existingThemeDict != null)
                {
                    app.Resources.MergedDictionaries.Remove(existingThemeDict);
                    app.Resources.MergedDictionaries.Add(newThemeDict);
                }
                else
                {
                    app.Resources.MergedDictionaries.Add(newThemeDict);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Theme Error: {ex.Message}");
            }
        }
    }
}