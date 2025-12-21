using LexiScan.Core.Models;
using LexiScan.Core.Services; // Đã thấy được do SettingsService giờ nằm ở Core
using LexiScan.Core.Enums;    // Đã thấy được Enum
using LexiScan.Core.Utils;
using LexiScanUI.Helpers;
using LexiScanService;
using System.Collections.ObjectModel;
using System.Windows.Input;
using InputKind = LexiScan.Core.Enums.InputType;
using UiTts = LexiScanService.TtsService;

namespace LexiScanUI.ViewModels
{
    public class PopupViewModel : BaseViewModel
    {
        private readonly TranslationService _translator;
        private readonly UiTts _ttsService;
        private readonly SettingsService _settingsService;

        // Properties (Giữ nguyên)
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
            _translator = new TranslationService();
            _ttsService = new UiTts();
            _settingsService = new SettingsService();

            PinCommand = new RelayCommand(ExecutePin);
            ReadAloudCommand = new RelayCommand(ExecuteReadAloud);
            SettingsCommand = new RelayCommand(ExecuteSettings);
            CloseCommand = new RelayCommand(ExecuteClose);
            ClickWordCommand = new RelayCommand(ExecuteClickWord);
            PinToFirebaseCommand = new RelayCommand(ExecutePinToFirebase);

        }

        // --- LOGIC XỬ LÝ DỮ LIỆU ---
        public void LoadTranslationData(TranslationResult result)
        {
            if (result == null) return;

            // 1. [TÍNH NĂNG] Bật/Tắt Popup
            var currentSettings = _settingsService.LoadSettings();
            if (!currentSettings.IsAutoReadEnabled) return; // Nếu tắt -> Dừng luôn

            IsSelectionMode = false;
            OriginalSentence = result.OriginalText ?? "";
            CurrentWord = result.OriginalText ?? "";
            CurrentTranslatedText = result.TranslatedText ?? "";
            Phonetic = (result.InputType == InputKind.SingleWord && !string.IsNullOrEmpty(result.Phonetic)) ? $"/{result.Phonetic}/" : "";

            Meanings.Clear();
            if (result.InputType == InputKind.SingleWord && result.Meanings != null)
                foreach (var m in result.Meanings) Meanings.Add(m);

            PrepareWordsForSelection();

            // 2. [TÍNH NĂNG] Tự động đọc khi dịch xong
            if (currentSettings.AutoPronounceOnTranslate)
            {
                ExecuteReadAloud(null);
            }
        }

        private void ExecuteReadAloud(object? parameter)
        {
            var txt = !string.IsNullOrWhiteSpace(CurrentWord) ? CurrentWord : CurrentTranslatedText;
            if (string.IsNullOrWhiteSpace(txt)) return;

            var settings = _settingsService.LoadSettings();

            double speed = 0;
            switch (settings.Speed)
            {
                case SpeechSpeed.Slower: speed = -5; break;
                case SpeechSpeed.Slow: speed = -3; break;
                case SpeechSpeed.Normal: speed = 0; break;
            }

            string accent = (settings.Voice == SpeechVoice.EngUK) ? "en-GB" : "en-US";
            _ttsService.Speak(txt, speed, accent);
        }
        // --- CÁC HÀM PHỤ TRỢ ---
        private void ExecutePin(object? parameter) => IsSelectionMode = !IsSelectionMode;

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

            // 4. [TÍNH NĂNG] Tự động đọc khi tra từ đơn (Click vào từ)
            if (_settingsService.LoadSettings().AutoPronounceOnLookup)
                ExecuteReadAloud(null);
        }

        private void ExecuteSettings(object? parameter)
        {
            GlobalEvents.RaiseRequestOpenSettings();
            IsSelectionMode = false; WordList.Clear();
        }
        private void ExecuteClose(object? parameter) { IsSelectionMode = false; WordList.Clear(); }
        private void ExecutePinToFirebase(object? parameter)
        {
            // Toggle trạng thái pin
            IsPinned = !IsPinned;

            // TODO: Lưu Firebase sau nếu cần:
            // if (IsPinned) FirebaseStore.Save(CurrentWord);
            // else FirebaseStore.Delete(CurrentWord);
        }

    }
}