using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
using LexiScan.Core.Utils;
using LexiScanData.Services;
using LexiScanUI.Helpers;
using System.Collections.ObjectModel;
using System.Speech.Synthesis;
using System.Windows.Input;
using InputKind = LexiScan.Core.Enums.InputType;
using System.Text;


namespace LexiScanUI.ViewModels
{
    public class PopupViewModel : BaseViewModel
    {
        private readonly TranslationService _translator;
        private readonly SettingsService _settingsService;
        private DatabaseServices? _dbService;

        private SpeechSynthesizer _synthesizer;
        private bool _isPlaying;

        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(); }
        }

        private string _currentWord = "";
        public string CurrentWord { get => _currentWord; set { _currentWord = value; OnPropertyChanged(); } }
        private string _phonetic = "";
        public string Phonetic { get => _phonetic; set { _phonetic = value; OnPropertyChanged(); } }
        private string _currentTranslatedText = "";
        public string CurrentTranslatedText { get => _currentTranslatedText; set { _currentTranslatedText = value; OnPropertyChanged(); } }
        private bool _isSelectionMode;
        public bool IsSelectionMode { get => _isSelectionMode; set { _isSelectionMode = value; OnPropertyChanged(); } }
        private string _originalSentence = "";
        public string OriginalSentence { get => _originalSentence; set { _originalSentence = value; OnPropertyChanged(); } }
        private bool _isPinned;
        public bool IsPinned
        {
            get => _isPinned;
            set { _isPinned = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Meaning> Meanings { get; } = new();
        public ObservableCollection<SelectableWord> WordList { get; } = new();

        public ICommand PinCommand { get; }
        public ICommand ReadAloudCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ClickWordCommand { get; }
        public ICommand PinToFirebaseCommand { get; }

        public PopupViewModel()
        {
            PinToFirebaseCommand = new RelayCommand(ExecutePinToFirebase);
            _translator = new TranslationService();
            _settingsService = new SettingsService();

            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
            _synthesizer.SpeakCompleted += (s, e) => IsPlaying = false;

            string uid = SessionManager.CurrentUserId;
            string token = SessionManager.CurrentAuthToken;
            if (!string.IsNullOrEmpty(uid) || !string.IsNullOrEmpty(token)) _dbService = new DatabaseServices(uid, SessionManager.CurrentAuthToken);

            PinToFirebaseCommand = new RelayCommand(ExecutePinToFirebase);
            PinCommand = new RelayCommand(ExecutePin);
            ReadAloudCommand = new RelayCommand(ExecuteReadAloud);
            SettingsCommand = new RelayCommand(ExecuteSettings);
            CloseCommand = new RelayCommand(ExecuteClose);
            ClickWordCommand = new RelayCommand(ExecuteClickWord);
        }

        // --- LOGIC XỬ LÝ DỮ LIỆU ---
        public async void LoadTranslationData(TranslationResult result)
        {
            if (result == null) return;

            StopAudio();

            var currentSettings = _settingsService.LoadSettings();

            IsSelectionMode = false;

            // Áp dụng chuẩn hóa Text
            OriginalSentence = NormalizeText(result.OriginalText);
            CurrentWord = NormalizeText(result.OriginalText);
            CurrentTranslatedText = NormalizeText(result.TranslatedText);

            Phonetic = (result.InputType == InputKind.SingleWord && !string.IsNullOrEmpty(result.Phonetic)) ? $"/{result.Phonetic}/" : "";

            Meanings.Clear();
            if (result.InputType == InputKind.SingleWord && result.Meanings != null)
            {
                foreach (var m in result.Meanings)
                {
                    // Chuẩn hóa PartOfSpeech
                    if (!string.IsNullOrEmpty(m.PartOfSpeech))
                        m.PartOfSpeech = NormalizeText(m.PartOfSpeech);

                    // Chuẩn hóa danh sách Definitions
                    if (m.Definitions != null && m.Definitions.Count > 0)
                    {
                        var normDefs = new List<string>();
                        foreach (var def in m.Definitions)
                        {
                            normDefs.Add(NormalizeText(def));
                        }
                        m.Definitions = normDefs;
                    }

                    Meanings.Add(m);
                }
            }

            PrepareWordsForSelection();

            if (currentSettings.AutoPronounceOnTranslate)
            {
                ExecuteReadAloud(null);
            }

            if (_dbService != null)
            {
                IsPinned = false;
                string wordToCheck = !string.IsNullOrEmpty(CurrentWord) ? CurrentWord : OriginalSentence;
                string? key = await _dbService.FindSavedKeyAsync(wordToCheck);
                if (key != null) IsPinned = true;
            }

            if (_dbService == null && (!string.IsNullOrEmpty(SessionManager.CurrentUserId) || !string.IsNullOrEmpty(SessionManager.CurrentAuthToken)))
            {
                _dbService = new DatabaseServices(SessionManager.CurrentUserId, SessionManager.CurrentAuthToken);
            }

            if (_dbService != null)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(OriginalSentence) && !string.IsNullOrWhiteSpace(CurrentTranslatedText))
                    {
                        await _dbService.AddHistoryAsync(new Sentences
                        {
                            SourceText = OriginalSentence,
                            TranslatedText = CurrentTranslatedText,
                            CreatedDate = System.DateTime.Now
                        });
                    }
                }
                catch { }
            }
        }

        // Hàm chuẩn hóa tiếng Việt
        private string NormalizeText(string? input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Normalize(NormalizationForm.FormC);
        }

        private void ExecuteReadAloud(object? parameter)
        {
            if (IsPlaying)
            {
                StopAudio();
                return;
            }

            var txt = !string.IsNullOrWhiteSpace(CurrentWord) ? CurrentWord : CurrentTranslatedText;
            if (string.IsNullOrWhiteSpace(txt)) return;

            var settings = _settingsService.LoadSettings();

            int speedRate = 0;
            switch (settings.Speed)
            {
                case SpeechSpeed.Slower: speedRate = -5; break;
                case SpeechSpeed.Slow: speedRate = -3; break;
                case SpeechSpeed.Normal: speedRate = 0; break;
            }
            _synthesizer.Rate = speedRate;

            string culture = (settings.Voice == SpeechVoice.EngUK) ? "en-GB" : "en-US";
            try
            {
                _synthesizer.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.NotSet, 0, new System.Globalization.CultureInfo(culture));
            }
            catch { }

            IsPlaying = true;
            _synthesizer.SpeakAsync(txt);
        }

        public void StopAudio()
        {
            if (_synthesizer != null && _synthesizer.State == SynthesizerState.Speaking)
            {
                _synthesizer.SpeakAsyncCancelAll();
            }
            IsPlaying = false;
        }

        private void ExecutePin(object? parameter)
        {
            IsSelectionMode = !IsSelectionMode;
        }

        private void PrepareWordsForSelection()
        {
            WordList.Clear();
            if (string.IsNullOrWhiteSpace(OriginalSentence)) return;
            foreach (var w in OriginalSentence.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var clean = w.Trim(',', '.', '?', '!', ';', ':');
                if (!string.IsNullOrWhiteSpace(clean)) WordList.Add(new SelectableWord(clean, ClickWordCommand));
            }
        }

        private async void ExecuteClickWord(object? parameter)
        {
            if (parameter is not string word) return;

            var result = await _translator.ProcessTranslationAsync(word);

            // Chuẩn hóa CurrentWord
            CurrentWord = NormalizeText(result.OriginalText ?? word);
            Phonetic = !string.IsNullOrEmpty(result.Phonetic) ? $"/{result.Phonetic}/" : "";

            Meanings.Clear();
            if (result.Meanings != null)
            {
                foreach (var m in result.Meanings)
                {
                    // Chuẩn hóa PartOfSpeech
                    if (!string.IsNullOrEmpty(m.PartOfSpeech))
                        m.PartOfSpeech = NormalizeText(m.PartOfSpeech);

                    // Chuẩn hóa danh sách Definitions
                    if (m.Definitions != null && m.Definitions.Count > 0)
                    {
                        var normDefs = new List<string>();
                        foreach (var def in m.Definitions)
                        {
                            normDefs.Add(NormalizeText(def));
                        }
                        m.Definitions = normDefs;
                    }
                    Meanings.Add(m);
                }
            }

            if (_settingsService.LoadSettings().AutoPronounceOnLookup)
                ExecuteReadAloud(null);
        }

        private void ExecuteSettings(object? parameter)
        {
            GlobalEvents.RaiseRequestOpenSettings();
            IsSelectionMode = false; WordList.Clear();
        }

        private void ExecuteClose(object? parameter)
        {
            StopAudio();
            IsSelectionMode = false;
            WordList.Clear();
        }

        private async void ExecutePinToFirebase(object? parameter)
        {
            if (_dbService == null)
            {
                if (!string.IsNullOrEmpty(SessionManager.CurrentUserId) || !string.IsNullOrEmpty(SessionManager.CurrentAuthToken))
                    _dbService = new DatabaseServices(SessionManager.CurrentUserId, SessionManager.CurrentAuthToken);
                else return;
            }

            string textToSave = !string.IsNullOrWhiteSpace(CurrentWord) ? CurrentWord : OriginalSentence;
            string meaningToSave = CurrentTranslatedText;

            if (string.IsNullOrWhiteSpace(textToSave)) return;

            try
            {
                string? existingKey = await _dbService.FindSavedKeyAsync(textToSave);

                if (existingKey != null)
                {
                    await _dbService.DeleteSavedItemAsync(existingKey);
                    IsPinned = false;
                }
                else
                {
                    await _dbService.SaveSimpleVocabularyAsync(textToSave, meaningToSave);
                    IsPinned = true;
                }

                GlobalEvents.RaisePersonalDictionaryUpdated();
            }
            catch { }
        }
    }
}