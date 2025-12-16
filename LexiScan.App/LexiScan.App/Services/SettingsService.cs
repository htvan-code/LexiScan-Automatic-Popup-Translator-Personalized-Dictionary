using LexiScan.App.Models; // SỬA: Chỉ using ngắn gọn thế này là đủ
using Newtonsoft.Json;
using System;
using System.IO;

namespace LexiScan.App.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appDirectory = Path.Combine(appDataPath, "LexiScan");

            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            _settingsFilePath = Path.Combine(appDirectory, "settings.json");
        }

        public Settings LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    // SỬA LỖI NULL: Dùng '?? new Settings()' để đảm bảo không bao giờ trả về null
                    return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading settings: {ex.Message}");
                    return new Settings();
                }
            }

            return new Settings();
        }

        public void SaveSettings(Settings settings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}