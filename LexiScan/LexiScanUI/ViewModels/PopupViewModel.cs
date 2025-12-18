using LexiScan.Core.Models;
using LexiScan.Core.Services;
using LexiScanData.Services;
using LexiScanService;
using LexiScanUI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CoreTts = LexiScan.Core.Services.TtsService;
using InputKind = LexiScan.Core.Enums.InputType;
using UiTts = LexiScanService.TtsService;

namespace LexiScanUI.ViewModels
{
    public class PopupViewModel : BaseViewModel
    {
        private readonly TranslationService _translator;
        private readonly DatabaseServices _dbService;
        private readonly UiTts _ttsService;


        // ===================== PROPERTIES =====================
        private string _currentWord = "";
        public string CurrentWord
        {
            get => _currentWord;
            set { _currentWord = value; OnPropertyChanged(); }
        }

        private string _phonetic = "";
        public string Phonetic
        {
            get => _phonetic;
            set { _phonetic = value; OnPropertyChanged(); }
        }

        private string _currentTranslatedText = "";
        public string CurrentTranslatedText
        {
            get => _currentTranslatedText;
            set { _currentTranslatedText = value; OnPropertyChanged(); }
        }

        private bool _isSelectionMode;
        public bool IsSelectionMode
        {
            get => _isSelectionMode;
            set { _isSelectionMode = value; OnPropertyChanged(); }
        }

        private string _originalSentence = "";
        public string OriginalSentence
        {
            get => _originalSentence;
            set { _originalSentence = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Meaning> Meanings { get; } = new();
        public ObservableCollection<SelectableWord> WordList { get; } = new();

        // ===================== COMMANDS =====================
        public ICommand PinCommand { get; }
        public ICommand ReadAloudCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ClickWordCommand { get; }

        // ===================== CONSTRUCTOR =====================
        public PopupViewModel()
        {
            _translator = new TranslationService();
            _dbService = new DatabaseServices();
            _ttsService = new UiTts();


            PinCommand = new RelayCommand(ExecutePin);
            ReadAloudCommand = new RelayCommand(ExecuteReadAloud);
            SettingsCommand = new RelayCommand(ExecuteSettings);
            CloseCommand = new RelayCommand(ExecuteClose);
            ClickWordCommand = new RelayCommand(ExecuteClickWord);
        }

        // ===================== LOAD TRANSLATION =====================
        public void LoadTranslationData(TranslationResult result)
        {
            if (result == null) return;

            // store original English sentence
            OriginalSentence = result.OriginalText ?? "";

            // header show English
            CurrentWord = result.OriginalText ?? "";

            // show Vietnamese
            CurrentTranslatedText = result.TranslatedText ?? "";

            // show phonetic only for single word
            Phonetic = (result.InputType == InputKind.SingleWord && !string.IsNullOrEmpty(result.Phonetic))
                ? $"/{result.Phonetic}/"
                : "";

            // meanings only for word
            Meanings.Clear();
            if (result.InputType == InputKind.SingleWord && result.Meanings != null)
            {
                foreach (var m in result.Meanings)
                    Meanings.Add(m);
            }

            // prepare selectable words for sentence
            PrepareWordsForSelection();
        }

        // ===================== PIN TOGGLE =====================
        private void ExecutePin(object? parameter)
        {
            IsSelectionMode = !IsSelectionMode;
        }

        // ===================== WORD SPLIT =====================
        private void PrepareWordsForSelection()
        {
            WordList.Clear();
            if (string.IsNullOrWhiteSpace(OriginalSentence))
                return;

            var parts = OriginalSentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var w in parts)
            {
                var clean = w.Trim(',', '.', '?', '!', ';', ':');
                if (!string.IsNullOrWhiteSpace(clean))
                    WordList.Add(new SelectableWord(clean, ClickWordCommand));
            }
        }

        // ===================== CLICK WORD TO TRANSLATE =====================
        private async void ExecuteClickWord(object? parameter)
        {
            if (parameter is not string word)
                return;

            var result = await _translator.ProcessTranslationAsync(word);

            CurrentWord = result.OriginalText ?? word;

            Phonetic = !string.IsNullOrEmpty(result.Phonetic)
                ? $"/{result.Phonetic}/"
                : "";

            Meanings.Clear();
            if (result.Meanings != null)
            {
                foreach (var m in result.Meanings)
                    Meanings.Add(m);
            }
        }

        // ===================== READ ALOUD =====================
        private void ExecuteReadAloud(object? parameter)
        {
            var txt = !string.IsNullOrWhiteSpace(CurrentWord) ? CurrentWord : CurrentTranslatedText;
            if (!string.IsNullOrWhiteSpace(txt))
                _ttsService.ReadText(txt);
        }

        private void ExecuteSettings(object? parameter) { }

        private void ExecuteClose(object? parameter)
        {
            IsSelectionMode = false;
            WordList.Clear();
        }
    }
}
