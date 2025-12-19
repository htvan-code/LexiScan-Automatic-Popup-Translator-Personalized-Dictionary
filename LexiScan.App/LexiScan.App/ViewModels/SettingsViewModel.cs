using LexiScan.App.Commands;
using LexiScan.App.Models;
using LexiScan.App.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LexiScan.App.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly SettingsService _settingsService = new SettingsService();
        private Settings _originalSettings;
        private Settings _currentSettings;
        private bool _hasUnsavedChanges;

        // Variables for Hotkey
        private bool _isChangingHotkey;
        private string _hotkeyButtonText = "Thiết Lập";

        public SettingsViewModel()
        {
            var loaded = _settingsService.LoadSettings();
            CurrentSettings = loaded;

            // Áp dụng theme
            ApplyTheme(CurrentSettings.IsDarkModeEnabled);

            SaveCommand = new RelayCommand(SaveSettings);
            CancelCommand = new RelayCommand(CancelChanges);

            // Logic bấm nút đổi hotkey
            ChangeHotkeyCommand = new RelayCommand((o) => {
                IsChangingHotkey = true;
                HotkeyButtonText = "Đang bấm phím...";
            });

            ExportDataCommand = new RelayCommand(_ => { });
        }

        // ... (Các Property CurrentSettings, IsDarkModeEnabled, HasUnsavedChanges giữ nguyên như bản cũ của bạn) ...
        // (Tôi rút gọn phần này để tập trung vào logic mới)

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

        // Property cho Hotkey
        public bool IsChangingHotkey { get => _isChangingHotkey; set { _isChangingHotkey = value; OnPropertyChanged(); } }
        public string HotkeyButtonText { get => _hotkeyButtonText; set { _hotkeyButtonText = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand ChangeHotkeyCommand { get; }

        // --- LOGIC HOTKEY ---
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

        // --- CÁC HÀM CƠ BẢN ---
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
            HasUnsavedChanges = !_currentSettings.Equals(_originalSettings);
        }

        private void SaveSettings(object parameter)
        {
            _settingsService.SaveSettings(_currentSettings);
            _originalSettings = (Settings)_currentSettings.Clone();
            CheckIfDirty();
            MessageBox.Show("Đã lưu!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelChanges(object parameter)
        {
            var backup = _originalSettings;
            _currentSettings.PropertyChanged -= OnSettingsChanged;

            // Restore manual
            _currentSettings.Hotkey = backup.Hotkey;
            _currentSettings.IsDarkModeEnabled = backup.IsDarkModeEnabled;
            // ... restore các biến khác ...

            _currentSettings.PropertyChanged += OnSettingsChanged;
            ApplyTheme(_currentSettings.IsDarkModeEnabled);
            OnPropertyChanged(nameof(IsDarkModeEnabled));
            CheckIfDirty();
        }

        // --- HÀM ĐỔI MÀU (FIX LỖI MATERIAL DESIGN) ---
        private void ApplyTheme(bool isDark)
        {
            var app = Application.Current;
            if (app == null) return;

            string assemblyName = "LexiScan"; // Chắc chắn tên này đúng với Properties của project
            string themePath = isDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
            string newUriString = $"pack://application:,,,/{assemblyName};component/{themePath}";

            try
            {
                ResourceDictionary? existingThemeDict = null;
                // Tìm file theme cũ (Light hoặc Dark) để thay thế
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