using System.Collections.ObjectModel;
using System.Windows.Input;
using LexiScan.App.Commands;

using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services; 

namespace LexiScan.App.ViewModels
{
    public class DictionaryViewModel : BaseViewModel
    {
        private readonly AppCoordinator _coordinator;
        private readonly SettingsService _settingsService;

        private string _searchText;
        private string _displayWord;
        private string _definitionText;
        private string _phoneticText;
        private string _selectedSuggestion;

        private bool _isListening;
        public bool IsListening { get => _isListening; set { _isListening = value; OnPropertyChanged(); } }

        private bool _isSpeaking;
        public bool IsSpeaking { get => _isSpeaking; set { _isSpeaking = value; OnPropertyChanged(); } }

        private double _voiceLevel;
        public double VoiceLevel
        {
            get => _voiceLevel;
            set { _voiceLevel = value; OnPropertyChanged(); }
        }
        public DictionaryViewModel(AppCoordinator coordinator)
        {
            _coordinator = coordinator;
            _settingsService = new SettingsService();

            _coordinator.TranslationCompleted += OnTranslationResultReceived;
            SuggestionList = new ObservableCollection<string>();

            _coordinator.VoiceRecognitionStarted += () => IsSpeaking = true;

            // Khi ngừng nghe
            _coordinator.VoiceRecognitionEnded += () => { IsSpeaking = false; IsListening = false; };
            
            SearchCommand = new RelayCommand(async (o) =>
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return;
                SuggestionList.Clear();
                await _coordinator.ExecuteSearchAsync(SearchText);
            });

            StartVoiceSearchCommand = new RelayCommand((o) =>
            {
                IsListening = true;
                _coordinator.StartVoiceSearch();
            });

            SpeakResultCommand = new RelayCommand(ExecuteSpeakResult);

            _coordinator.VoiceSearchCompleted += (text) =>
            {
                IsListening = false;
                IsSpeaking = false;
                SearchText = text;
                SearchCommand.Execute(null); 
            };

            _coordinator.AudioLevelUpdated += (level) =>
            {
                double newSize = 25.0 + (level * 4.0);

                if (newSize < 25) newSize = 25;
                if (newSize > 120) newSize = 120;

                VoiceLevel = newSize;
            };

        }

        public ObservableCollection<string> SuggestionList { get; set; }
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    if (!string.IsNullOrWhiteSpace(_searchText)) LoadSuggestions(_searchText);
                    else SuggestionList.Clear();
                }
            }
        }
        public string SelectedSuggestion
        {
            get => _selectedSuggestion;
            set
            {
                _selectedSuggestion = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(value))
                {
                    SearchText = value;
                    SuggestionList.Clear();
                    _coordinator.ExecuteSearchAsync(value);
                }
            }
        }
        public string DisplayWord { get => _displayWord; set { _displayWord = value; OnPropertyChanged(); } }
        public string DefinitionText { get => _definitionText; set { _definitionText = value; OnPropertyChanged(); } }
        public string PhoneticText { get => _phoneticText; set { _phoneticText = value; OnPropertyChanged(); } }

        public ICommand SearchCommand { get; }
        public ICommand StartVoiceSearchCommand { get; }
        public ICommand SpeakResultCommand { get; }

        private async void LoadSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) { SuggestionList.Clear(); return; }
            var suggestions = await _coordinator.GetRecommendWordsAsync(query);
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                SuggestionList.Clear();
                foreach (var s in suggestions) SuggestionList.Add(s);
            });
        }
        //Thịnh sửa 20_12
        private void OnTranslationResultReceived(TranslationResult result)
        {
            // 1. Kiểm tra null như bên Popup
            if (result == null) return;

            // 2. Gán dữ liệu cơ bản
            DisplayWord = result.OriginalText ?? "";

            // [LOGIC GIỐNG POPUP] Thêm dấu gạch chéo /.../ cho phiên âm nếu có
            PhoneticText = (!string.IsNullOrEmpty(result.Phonetic)) ? $"/{result.Phonetic}/" : "";

            // 3. Xử lý hiển thị Nghĩa (Dùng StringBuilder vì giao diện chính là TextBlock)
            var sb = new System.Text.StringBuilder();

            // - Nghĩa dịch tóm tắt (Google Translate)
            if (!string.IsNullOrWhiteSpace(result.TranslatedText))
            {
                sb.AppendLine(result.TranslatedText);
            }

            // - Nghĩa chi tiết (Noun/Verb) - Logic lấy dữ liệu giống Popup nhưng chuyển thành text
            if (result.Meanings != null && result.Meanings.Count > 0)
            {
                sb.AppendLine(); // Xuống dòng tạo khoảng cách
                foreach (var m in result.Meanings)
                {
                    // In loại từ (Ví dụ: ★ danh từ)
                    sb.AppendLine($"★ {m.PartOfSpeech}");

                    // In các định nghĩa con
                    foreach (var def in m.Definitions)
                    {
                        sb.AppendLine($"   - {def}");
                    }
                    sb.AppendLine(); // Dòng trống cho thoáng
                }
            }

            // Gán chuỗi đã gộp vào biến hiển thị
            DefinitionText = sb.ToString().Trim();

            var currentSettings = _settingsService.LoadSettings();

            if (currentSettings.AutoPronounceOnLookup)
            {
                // Gọi hàm đọc (MainViewModel đã có sẵn hàm này)
                ExecuteSpeakResult(null);
            }
        }

        // --- LOGIC CHUYỂN ĐỔI ENUM SANG SỐ LIỆU ---
        private void ExecuteSpeakResult(object obj)
        {
            string textToRead = !string.IsNullOrWhiteSpace(DisplayWord) ? DisplayWord : SearchText;
            if (string.IsNullOrWhiteSpace(textToRead)) return;

            // QUAN TRỌNG: Phải dùng SettingsService từ LexiScan.Core
            // LoadSettings() sẽ đọc file json mới nhất mà SettingsView vừa lưu
            var settings = _settingsService.LoadSettings();

            // 1. Chuyển đổi tốc độ (Rate)
            double speedRate = 0;
            switch (settings.Speed)
            {
                case SpeechSpeed.Slower: speedRate = -5; break;
                case SpeechSpeed.Slow: speedRate = -3; break;
                case SpeechSpeed.Normal: speedRate = 0; break;
            }

            // 2. Chuyển đổi giọng (Voice)
            string accent = (settings.Voice == SpeechVoice.EngUK) ? "en-GB" : "en-US";

            // 3. Thực hiện đọc
            _coordinator.Speak(textToRead, speedRate, accent);
        }
    }
}