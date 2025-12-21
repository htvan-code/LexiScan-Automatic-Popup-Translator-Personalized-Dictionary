using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LexiScan.Core.Enums;

namespace LexiScan.Core.Models
{
    public class Settings : ICloneable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- CÁC BIẾN CÀI ĐẶT ---
        private string _hotkey = "Ctrl + Space";
        public string Hotkey
        {
            get => _hotkey;
            set { if (_hotkey != value) { _hotkey = value; OnPropertyChanged(); } }
        }

        private bool _isAutoReadEnabled = true; // Biến Bật/Tắt Popup
        public bool IsAutoReadEnabled
        {
            get => _isAutoReadEnabled;
            set { if (_isAutoReadEnabled != value) { _isAutoReadEnabled = value; OnPropertyChanged(); } }
        }

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

        private bool _autoPronounceOnLookup = false;
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

        private bool _isDarkModeEnabled = false;
        public bool IsDarkModeEnabled
        {
            get => _isDarkModeEnabled;
            set { if (_isDarkModeEnabled != value) { _isDarkModeEnabled = value; OnPropertyChanged(); } }
        }

        private bool _autoSaveHistoryToDictionary = false;
        public bool AutoSaveHistoryToDictionary
        {
            get => _autoSaveHistoryToDictionary;
            set { if (_autoSaveHistoryToDictionary != value) { _autoSaveHistoryToDictionary = value; OnPropertyChanged(); } }
        }

        public object Clone() => this.MemberwiseClone();
    }
}