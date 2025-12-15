using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace LexiScan.App.Models
{
    // Enum giữ nguyên
    public enum SpeechSpeed { Slower, Slow, Normal }
    public enum SpeechVoice { EngUK, EngUS }

    // Thêm INotifyPropertyChanged để báo hiệu thay đổi và ICloneable để copy
    public class Settings : INotifyPropertyChanged, ICloneable
    {
        // --- 1. Phím Tắt & Tương Tác ---
        private string _hotkey = "Ctrl + Space";
        public string Hotkey
        {
            get => _hotkey;
            set { if (_hotkey != value) { _hotkey = value; OnPropertyChanged(); } }
        }

        private bool _showScanIcon = true;
        public bool ShowScanIcon
        {
            get => _showScanIcon;
            set { if (_showScanIcon != value) { _showScanIcon = value; OnPropertyChanged(); } }
        }

        // --- 2. Phát Âm ---
        private SpeechSpeed _speed = SpeechSpeed.Normal;
        public SpeechSpeed Speed
        {
            get => _speed;
            set { if (_speed != value) { _speed = value; OnPropertyChanged(); } }
        }

        private SpeechVoice _voice = SpeechVoice.EngUS;
        public SpeechVoice Voice
        {
            get => _voice;
            set { if (_voice != value) { _voice = value; OnPropertyChanged(); } }
        }

        private bool _autoPronounceOnLookup = true;
        public bool AutoPronounceOnLookup
        {
            get => _autoPronounceOnLookup;
            set { if (_autoPronounceOnLookup != value) { _autoPronounceOnLookup = value; OnPropertyChanged(); } }
        }

        private bool _autoPronounceOnTranslate = false;
        public bool AutoPronounceOnTranslate
        {
            get => _autoPronounceOnTranslate;
            set { if (_autoPronounceOnTranslate != value) { _autoPronounceOnTranslate = value; OnPropertyChanged(); } }
        }

        private bool _isAutoReadEnabled = true;
        public bool IsAutoReadEnabled
        {
            get => _isAutoReadEnabled;
            set { if (_isAutoReadEnabled != value) { _isAutoReadEnabled = value; OnPropertyChanged(); } }
        }

        // --- 3. Giao Diện ---
        private bool _isDarkModeEnabled = true;
        public bool IsDarkModeEnabled
        {
            get => _isDarkModeEnabled;
            set { if (_isDarkModeEnabled != value) { _isDarkModeEnabled = value; OnPropertyChanged(); } }
        }

        // --- 4. Quản Lý Dữ Liệu ---
        private bool _autoSaveHistoryToDictionary = true;
        public bool AutoSaveHistoryToDictionary
        {
            get => _autoSaveHistoryToDictionary;
            set { if (_autoSaveHistoryToDictionary != value) { _autoSaveHistoryToDictionary = value; OnPropertyChanged(); } }
        }

        // --- CÁC HÀM HỖ TRỢ LOGIC ---

        // Hàm copy object (để tạo bản backup)
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        // Hàm so sánh xem 2 Settings có giống hệt nhau không
        public override bool Equals(object obj)
        {
            if (obj is not Settings other) return false;

            return Hotkey == other.Hotkey &&
                   ShowScanIcon == other.ShowScanIcon &&
                   Speed == other.Speed &&
                   Voice == other.Voice &&
                   AutoPronounceOnLookup == other.AutoPronounceOnLookup &&
                   AutoPronounceOnTranslate == other.AutoPronounceOnTranslate &&
                   IsAutoReadEnabled == other.IsAutoReadEnabled &&
                   IsDarkModeEnabled == other.IsDarkModeEnabled &&
                   AutoSaveHistoryToDictionary == other.AutoSaveHistoryToDictionary;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hotkey, Speed, Voice, IsDarkModeEnabled);
        }

        // Sự kiện thông báo
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}