using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace LexiScan.App.Models
{

    namespace LexiScan.App.Models
    {
        // Enum cho Tốc độ đọc (được sử dụng nội bộ bởi Service)
        public enum SpeechSpeed
        {
            Slower,
            Slow,
            Normal
        }

        // Enum cho Giọng đọc (được sử dụng nội bộ bởi Service)
        public enum SpeechVoice
        {
            EngUK,
            EngUS
        }

        // Lớp chứa tất cả các cấu hình ứng dụng.
        // Các thuộc tính này sẽ được serialize (lưu) vào file JSON.
        public class Settings
        {
            // 1. Phím Tắt & Tương Tác
            public string Hotkey { get; set; } = "Ctrl + Space";
            public bool ShowScanIcon { get; set; } = true;

            // 2. Phát Âm (Sử dụng Enum để lưu trữ)
            public SpeechSpeed Speed { get; set; } = SpeechSpeed.Normal;
            public SpeechVoice Voice { get; set; } = SpeechVoice.EngUS;
            public bool AutoPronounceOnLookup { get; set; } = true;
            public bool AutoPronounceOnTranslate { get; set; } = false;
            public bool IsAutoReadEnabled { get; set; } = true;
            // 3. Giao Diện
            public bool IsDarkModeEnabled { get; set; } = true;

            // 4. Quản Lý Dữ Liệu
            public bool AutoSaveHistoryToDictionary { get; set; } = true;
        }
    }
}
