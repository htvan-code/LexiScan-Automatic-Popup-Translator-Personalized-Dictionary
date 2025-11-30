using System;
using System.Windows;

namespace LexiScan.App.Services
{
    // ThemeService quản lý việc hoán đổi ResourceDictionary toàn cục
    public class ThemeService
    {
        // Singleton Instance
        public static ThemeService Instance { get; } = new ThemeService();

        // Event để thông báo cho các Window biết khi nào theme thay đổi
        public event Action<string> ThemeChanged;

        // Phương thức gọi khi theme thay đổi (từ SettingsViewModel)
        public void ApplyTheme(string themeName)
        {
            // Kiểm tra themeName có phải là "Dark" hoặc "Light"
            if (themeName != "Dark" && themeName != "Light")
                return;

            // Thông báo cho tất cả các lắng nghe (MainWindow)
            ThemeChanged?.Invoke(themeName);
        }
    }
}