// File: Models/Settings.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexiScan.App.Models // Giữ lại namespace này
{
    // Đã XÓA định nghĩa namespace trùng lặp ở đây

    public class Settings
    {
        // Giao diện (Ánh xạ ComboBox/Theme)
        public int SelectedThemeIndex { get; set; } = 0; // 0=Light, 1=Dark

        // Phím tắt (Ánh xạ TextBox Hotkey)
        // Khắc phục CS8618 bằng cách khởi tạo string
        public string HotkeyCombination { get; set; } = "Ctrl+Shift+S";

        // Checkboxes
        public bool IsShowIconEnabled { get; set; } = true;
        public bool IsAutoReadEnabled { get; set; } = false;
        public bool IsHistoryEnabled { get; set; } = true;
    }
}