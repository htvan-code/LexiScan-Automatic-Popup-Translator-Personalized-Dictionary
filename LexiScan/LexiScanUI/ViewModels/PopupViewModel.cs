using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
using LexiScan.Core.Utils;
using LexiScanData.Services;
using LexiScanService;
using LexiScanUI.Helpers;
using System.Collections.ObjectModel;
using System.Speech.Synthesis; // [MỚI] Thêm thư viện này (nhớ Add Reference System.Speech nếu chưa có)
using System.Windows.Input;
using InputKind = LexiScan.Core.Enums.InputType;
// using UiTts = LexiScanService.TtsService; // [BỎ] Chúng ta sẽ dùng trực tiếp Synthesizer để dễ kiểm soát Stop/Start

namespace LexiScanUI.ViewModels
{
    public class PopupViewModel : BaseViewModel
    {
        private readonly TranslationService _translator;
        // private readonly UiTts _ttsService; // [BỎ] Không dùng service cũ nữa
        private readonly SettingsService _settingsService;
        private DatabaseServices? _dbService;

        // [MỚI] Đối tượng đọc và Biến trạng thái
        private SpeechSynthesizer _synthesizer;
        private bool _isPlaying;

        // [MỚI] Property để Binding màu nút Loa (True = Sáng, False = Tối)
        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(); }
        }

        // ... Các Properties cũ giữ nguyên ...
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
            // _ttsService = new UiTts(); // [BỎ]
            _settingsService = new SettingsService();

            // [MỚI] Khởi tạo bộ đọc và đăng ký sự kiện
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
            // Sự kiện: Khi đọc xong -> Tự set IsPlaying về false (Tắt đèn)
            _synthesizer.SpeakCompleted += (s, e) => IsPlaying = false;

            string uid = SessionManager.CurrentUserId;
            if (!string.IsNullOrEmpty(uid)) _dbService = new DatabaseServices(uid);

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

            // [MỚI] Reset trạng thái đọc khi load từ mới
            StopAudio();

            var currentSettings = _settingsService.LoadSettings();

            IsSelectionMode = false;
            OriginalSentence = result.OriginalText ?? "";
            CurrentWord = result.OriginalText ?? "";
            CurrentTranslatedText = result.TranslatedText ?? "";
            Phonetic = (result.InputType == InputKind.SingleWord && !string.IsNullOrEmpty(result.Phonetic)) ? $"/{result.Phonetic}/" : "";

            Meanings.Clear();
            if (result.InputType == InputKind.SingleWord && result.Meanings != null)
                foreach (var m in result.Meanings) Meanings.Add(m);

            PrepareWordsForSelection();

            if (currentSettings.AutoPronounceOnTranslate)
            {
                ExecuteReadAloud(null);
            }

            // Logic Ghim (Giữ nguyên)
            if (_dbService != null)
            {
                IsPinned = false;
                string wordToCheck = !string.IsNullOrEmpty(CurrentWord) ? CurrentWord : OriginalSentence;
                string? key = await _dbService.FindSavedKeyAsync(wordToCheck);
                if (key != null) IsPinned = true;
            }
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
            catch {  }

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

        // ... CÁC HÀM PHỤ TRỢ ...
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
            CurrentWord = result.OriginalText ?? word;
            Phonetic = !string.IsNullOrEmpty(result.Phonetic) ? $"/{result.Phonetic}/" : "";
            Meanings.Clear();
            if (result.Meanings != null) foreach (var m in result.Meanings) Meanings.Add(m);

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
                if (!string.IsNullOrEmpty(SessionManager.CurrentUserId))
                    _dbService = new DatabaseServices(SessionManager.CurrentUserId);
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
            }
            catch { }
        }
    }
}