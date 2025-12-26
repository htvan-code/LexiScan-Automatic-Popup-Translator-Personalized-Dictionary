using System;
using System.IO;
using System.Text.Json;
using LexiScan.Core.Models;

namespace LexiScan.Core.Services
{
    public class SettingsService
    {
        private readonly string _filePath;
        public SettingsService() => _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public Settings LoadSettings()
        {
            if (!File.Exists(_filePath)) return new Settings();
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
            catch
            {
                return new Settings(); // Lỗi thì trả về mặc định
            }
        }

        public void SaveSettings(Settings settings)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_filePath, JsonSerializer.Serialize(settings, options));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Save Error: " + ex.Message);
            }
        }
    }
}