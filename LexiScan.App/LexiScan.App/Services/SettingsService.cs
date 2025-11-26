// File: Services/SettingsService.cs

// Đã sửa: Chỉ cần using LexiScan.App.Models (nếu Settings nằm trong đó)
using LexiScan.App.Models;
using Newtonsoft.Json;
using System.IO;

namespace LexiScan.App.Services
{
    public class SettingsService
    {
        private const string SettingsFilePath = "settings.json";

        // Khắc phục CS8603/CS8600: Đảm bảo không bao giờ trả về null
        public Settings LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    // Sử dụng toán tử null-coalescing (??) để xử lý trường hợp DeserializeObject trả về null
                    var settings = JsonConvert.DeserializeObject<Settings>(json);
                    return settings ?? new Settings();
                }
                catch
                {
                    // Xử lý lỗi đọc/parse file JSON và trả về default settings
                    return new Settings();
                }
            }
            // Trả về một instance mới nếu file không tồn tại.
            return new Settings();
        }

        // Khắc phục CA1822: Giữ nguyên phương thức instance
        public void SaveSettings(Settings settings)
        {
            string json = JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(SettingsFilePath, json);
        }
    }
}