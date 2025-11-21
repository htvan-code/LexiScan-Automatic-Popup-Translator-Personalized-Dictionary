using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; 
using LexiScan.App.Models; 
using Newtonsoft.Json;



using System.ComponentModel;
using System.Windows.Input;
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
                // Deserialize: Đọc chuỗi JSON và chuyển thành đối tượng Settings
                string json = File.ReadAllText(SettingsFilePath);
                return JsonConvert.DeserializeObject<Settings>(json);
            }
            // Trả về cài đặt mặc định nếu chưa có file
            return new Settings();
        }

        public void SaveSettings(Settings settings)
        {
            // Serialize: Chuyển đối tượng Settings thành chuỗi JSON
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsFilePath, json);
        }
    }
}
