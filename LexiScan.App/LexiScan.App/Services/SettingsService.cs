using LexiScan.App.Models.LexiScan.App.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexiScan.App.Services
{
    // Lớp xử lý logic I/O cho cấu hình người dùng
    public class SettingsService
    {
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            // Thiết lập đường dẫn file cài đặt trong thư mục dữ liệu ứng dụng của người dùng
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appDirectory = Path.Combine(appDataPath, "LexiScan");

            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            _settingsFilePath = Path.Combine(appDirectory, "settings.json");
        }

        // Tải cài đặt từ file JSON
        public Settings LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    // Deserialization: Chuyển JSON thành đối tượng Settings
                    return JsonConvert.DeserializeObject<Settings>(json);
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi: Nếu file bị hỏng, in ra console và trả về cấu hình mặc định
                    Console.WriteLine($"Error loading settings: {ex.Message}");
                    return new Settings();
                }
            }

            // Nếu không tìm thấy file, trả về cấu hình mặc định
            return new Settings();
        }

        // Lưu cài đặt vào file JSON
        public void SaveSettings(Settings settings)
        {
            try
            {
                // Serialization: Chuyển đối tượng Settings thành chuỗi JSON
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(_settingsFilePath, json);
                Console.WriteLine("Settings successfully saved to settings.json");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi ghi file
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}