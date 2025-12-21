using System.Collections.ObjectModel;
using System.Windows.Input;
using LexiScan.App.Commands;
using LexiScan.App.Services;
using LexiScan.App.Models; 
using LexiScan.Core;
using LexiScan.Core.Models;

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
            DisplayWord = result.OriginalText;
            DefinitionText = result.TranslatedText;
            PhoneticText = result.Phonetic;

            var currentSettings = _settingsService.LoadSettings();

            // Kiểm tra nếu người dùng bật "Tự động phát âm khi tra từ"
            if (currentSettings.AutoPronounceOnLookup)
            {
                // Gọi hàm ExecuteSpeakResult để dùng chung logic tốc độ/giọng đọc
                ExecuteSpeakResult(null);
            }
        }

        // --- LOGIC CHUYỂN ĐỔI ENUM SANG SỐ LIỆU ---
        private void ExecuteSpeakResult(object obj)
        {
            if (string.IsNullOrWhiteSpace(DisplayWord)) return;

            // QUAN TRỌNG: Load cài đặt mới nhất từ file settings.json
            var settings = _settingsService.LoadSettings();

            // 1. Chuyển đổi tốc độ (Dùng số nguyên từ -10 đến 10)
            double speedRate = 0;
            switch (settings.Speed)
            {
                case SpeechSpeed.Slower: speedRate = -5; break; // Chậm hơn
                case SpeechSpeed.Slow: speedRate = -3; break; // Chậm
                case SpeechSpeed.Normal: speedRate = 0; break; // Bình thường
            }

            // 2. Chuyển đổi giọng đọc
            string accentCode = (settings.Voice == SpeechVoice.EngUK) ? "en-GB" : "en-US";

            // 3. Gọi Coordinator thực hiện đọc
            _coordinator.Speak(DisplayWord, speedRate, accentCode);
        }
    }
}