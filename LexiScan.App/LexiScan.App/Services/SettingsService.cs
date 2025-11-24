using Newtonsoft.Json;
using System.IO;
using LexiScan.App.Models.LexiScan.App.Models;
namespace LexiScan.App.Services
{
    public class SettingsService
    {
        private const string SettingsFilePath = "settings.json";

        public Settings LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {

                string json = File.ReadAllText(SettingsFilePath);
                return JsonConvert.DeserializeObject<Settings>(json);
            }
            return new Settings();
        }

        public void SaveSettings(Settings settings)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsFilePath, json);
        }
    }
}
