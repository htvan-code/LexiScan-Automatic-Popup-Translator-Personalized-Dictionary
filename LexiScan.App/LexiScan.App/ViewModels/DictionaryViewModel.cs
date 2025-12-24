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
        private readonly DatabaseServices _dbService;
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

            string uid = SessionManager.CurrentUserId;
            if (!string.IsNullOrEmpty(uid)) _dbService = new DatabaseServices(uid);

            _coordinator.TranslationCompleted += OnTranslationResultReceived;
            SuggestionList = new ObservableCollection<string>();

            _coordinator.VoiceRecognitionStarted += () => IsSpeaking = true;
            _coordinator.VoiceRecognitionEnded += () => { IsSpeaking = false; IsListening = false; };

            SearchCommand = new RelayCommand(async (o) =>
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return;
                SuggestionList.Clear();
                await _coordinator.ExecuteSearchAsync(SearchText);
            });

            // CẬP NHẬT: Gửi kèm nguồn là Dictionary
            StartVoiceSearchCommand = new RelayCommand((o) =>
            {
                IsListening = true;
                _coordinator.StartVoiceSearch(VoiceSource.Dictionary);
            });

            SpeakResultCommand = new RelayCommand(ExecuteSpeakResult);

            // CẬP NHẬT: Kiểm tra nguồn trước khi nhận kết quả
            _coordinator.VoiceSearchCompleted += (text) =>
            {
                if (_coordinator.CurrentVoiceSource == VoiceSource.Dictionary)
                {
                    IsListening = false;
                    IsSpeaking = false;
                    SearchText = text;
                    SearchCommand.Execute(null);
                }
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

        private void OnTranslationResultReceived(TranslationResult result)
        {
            if (result == null) return;

            DisplayWord = result.OriginalText ?? "";
            PhoneticText = (!string.IsNullOrEmpty(result.Phonetic)) ? $"/{result.Phonetic}/" : "";

            var sb = new System.Text.StringBuilder();

            if (!string.IsNullOrWhiteSpace(result.TranslatedText))
            {
                sb.AppendLine(result.TranslatedText);
            }

            if (result.Meanings != null && result.Meanings.Count > 0)
            {
                sb.AppendLine();
                foreach (var m in result.Meanings)
                {
                    sb.AppendLine($"★ {m.PartOfSpeech}");
                    foreach (var def in m.Definitions)
                    {
                        sb.AppendLine($"    - {def}");
                    }
                    sb.AppendLine();
                }
            }

            DefinitionText = sb.ToString().Trim();
            var currentSettings = _settingsService.LoadSettings();

            if (currentSettings.AutoPronounceOnLookup)
            {
                ExecuteSpeakResult(null);
            }
        }

        private void ExecuteSpeakResult(object obj)
        {
            string textToRead = !string.IsNullOrWhiteSpace(DisplayWord) ? DisplayWord : SearchText;
            if (string.IsNullOrWhiteSpace(textToRead)) return;

            var settings = _settingsService.LoadSettings();

            double speedRate = 0;
            switch (settings.Speed)
            {
                case SpeechSpeed.Slower: speedRate = -5; break;
                case SpeechSpeed.Slow: speedRate = -3; break;
                case SpeechSpeed.Normal: speedRate = 0; break;
            }

            string accent = (settings.Voice == SpeechVoice.EngUK) ? "en-GB" : "en-US";
            _coordinator.Speak(textToRead, speedRate, accent);
        }
    }
}