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
        private Settings _originalSettings; // Bản gốc (Backup)
        private Settings _currentSettings;  // Bản đang chỉnh sửa
        private bool _hasUnsavedChanges;    // Biến quyết định Ẩn/Hiện nút

        public SettingsViewModel()
        {
            // 1. Tải cài đặt lên
            var loaded = _settingsService.LoadSettings();

            // 2. Gán vào CurrentSettings (Logic tạo backup sẽ chạy trong setter)
            CurrentSettings = loaded;

            // 3. Áp dụng màu ngay lập tức dựa trên cài đặt vừa tải
            ApplyTheme(CurrentSettings.IsDarkModeEnabled);

            // 4. Khởi tạo Command
            SaveCommand = new RelayCommand(SaveSettings);
            CancelCommand = new RelayCommand(CancelChanges);

            // Các command khác
            ExportDataCommand = new RelayCommand(_ => { });
            ChangeHotkeyCommand = new RelayCommand(_ => { });
        }

        public Settings CurrentSettings
        {
            get => _currentSettings;
            set
            {
                // Hủy đăng ký sự kiện cũ để tránh lỗi
                if (_currentSettings != null)
                    _currentSettings.PropertyChanged -= OnSettingsChanged;

                _currentSettings = value;

                if (_currentSettings != null)
                {
                    // Tạo bản sao lưu ngay lập tức
                    _originalSettings = (Settings)_currentSettings.Clone();

                    // Đăng ký sự kiện: Khi user thay đổi bất cứ property nào
                    _currentSettings.PropertyChanged += OnSettingsChanged;
                }

                OnPropertyChanged();
                CheckIfDirty(); // Kiểm tra trạng thái nút bấm
            }
        }

        // Thuộc tính này Bind vào Checkbox DarkMode
        public bool IsDarkModeEnabled
        {
            get => _currentSettings.IsDarkModeEnabled;
            set
            {
                if (_currentSettings.IsDarkModeEnabled != value)
                {
                    _currentSettings.IsDarkModeEnabled = value;
                    OnPropertyChanged(); // Cập nhật UI

                    // Đổi màu ngay lập tức để người dùng xem trước
                    ApplyTheme(value);
                }
            }
        }

        // Thuộc tính này Bind vào Visibility của nút Lưu/Hủy
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                if (_hasUnsavedChanges != value)
                {
                    _hasUnsavedChanges = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand ChangeHotkeyCommand { get; }

        // --- CÁC HÀM LOGIC ---

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            // Nếu người dùng thay đổi DarkMode thông qua Checkbox
            if (e.PropertyName == nameof(Settings.IsDarkModeEnabled))
            {
                // Đảm bảo Property IsDarkModeEnabled được cập nhật (nếu cần)
                OnPropertyChanged(nameof(IsDarkModeEnabled));
                // Gọi hàm đổi màu
                ApplyTheme(_currentSettings.IsDarkModeEnabled);
            }

            CheckIfDirty();
        }

        private void CheckIfDirty()
        {
            if (_currentSettings == null || _originalSettings == null) return;

            // So sánh bản hiện tại với bản gốc. Nếu KHÁC nhau -> Hiện nút
            HasUnsavedChanges = !_currentSettings.Equals(_originalSettings);
        }

        private void SaveSettings(object parameter)
        {
            // 1. Lưu xuống file
            _settingsService.SaveSettings(_currentSettings);

            // 2. Cập nhật lại bản gốc thành bản hiện tại
            _originalSettings = (Settings)_currentSettings.Clone();

            // 3. Ẩn nút đi
            CheckIfDirty();

            MessageBox.Show("Cài đặt đã được lưu thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelChanges(object parameter)
        {
            // 1. Khôi phục dữ liệu từ bản gốc
            var backup = _originalSettings;

            // Ngắt sự kiện tạm thời để không trigger lung tung
            _currentSettings.PropertyChanged -= OnSettingsChanged;

            _currentSettings.Hotkey = backup.Hotkey;
            _currentSettings.ShowScanIcon = backup.ShowScanIcon;
            _currentSettings.Speed = backup.Speed;
            _currentSettings.Voice = backup.Voice;
            _currentSettings.AutoPronounceOnLookup = backup.AutoPronounceOnLookup;
            _currentSettings.AutoPronounceOnTranslate = backup.AutoPronounceOnTranslate;
            _currentSettings.IsAutoReadEnabled = backup.IsAutoReadEnabled;
            _currentSettings.IsDarkModeEnabled = backup.IsDarkModeEnabled;
            _currentSettings.AutoSaveHistoryToDictionary = backup.AutoSaveHistoryToDictionary;

            // Đăng ký lại sự kiện
            _currentSettings.PropertyChanged += OnSettingsChanged;

            // 2. Khôi phục màu sắc cũ (nếu lỡ đổi màu rồi bấm hủy)
            ApplyTheme(_currentSettings.IsDarkModeEnabled);
            OnPropertyChanged(nameof(IsDarkModeEnabled)); // Cập nhật lại Checkbox trên UI

            // 3. Ẩn nút đi
            CheckIfDirty();
        }

        // --- HÀM ĐỔI MÀU (CÁCH MỚI - ĐƠN GIẢN HƠN) ---
        private void ApplyTheme(bool isDark)
        {
            var app = Application.Current;
            if (app == null) return;

            // 1. Xác định tên Assembly (Tên Project)
            // Hãy kiểm tra xem Project bạn tên là "LexiScan" hay "LexiScan.App"
            // Dựa vào App.xaml của bạn, có vẻ tên là "LexiScan" (không có .App)
            string assemblyName = "LexiScan";

            // 2. Xác định đường dẫn file mới
            string themePath = isDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
            string newUriString = $"pack://application:,,,/{assemblyName};component/{themePath}";

            try
            {
                // 3. Tìm xem trong App.xaml đã có file Theme nào chưa (Light hoặc Dark)
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

                // 4. Tạo dictionary mới
                var newThemeDict = new ResourceDictionary
                {
                    Source = new Uri(newUriString, UriKind.Absolute)
                };

                // 5. Thực hiện thay thế
                if (existingThemeDict != null)
                {
                    // Nếu tìm thấy cái cũ -> Xóa cái cũ, thêm cái mới
                    app.Resources.MergedDictionaries.Remove(existingThemeDict);
                    app.Resources.MergedDictionaries.Add(newThemeDict);
                }
                else
                {
                    // Nếu chưa có -> Thêm mới vào
                    app.Resources.MergedDictionaries.Add(newThemeDict);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đổi màu: {ex.Message}\n\nKiểm tra lại tên Project trong biến 'assemblyName'.", "Lỗi Theme");
            }
        }
    }
}