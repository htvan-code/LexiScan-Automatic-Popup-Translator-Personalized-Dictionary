using LexiScan.App.ViewModels;
using LexiScanData.Services; 
using LexiScanService;
using LexiScanUI.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LexiScan.Core.Models; 
namespace LexiScanUI.ViewModels
{
    public class PopupViewModel : INotifyPropertyChanged
    {
        private string _currentWord;
        public string CurrentWord
        {
            get => _currentWord;
            set { _currentWord = value; OnPropertyChanged(); }
        }
        private string _phonetic;
        public string Phonetic
        {
            get => _phonetic;
            set { _phonetic = value; OnPropertyChanged(); }
        }

        private readonly DatabaseServices _dbService;
        private readonly TtsService _ttsService;

        public ObservableCollection<Meaning> Meanings { get; } = new ObservableCollection<Meaning>();
        public PopupViewModel()
        {
            _dbService = new DatabaseServices();
            _ttsService = new TtsService();


            CurrentSentenceId = 1;
            CurrentTranslatedText = "Học lập trình C#";
            PinCommand = new RelayCommand(ExecutePin);
            ReadAloudCommand = new RelayCommand(ExecuteReadAloud);
            SettingsCommand = new RelayCommand(ExecuteSettings);
            CloseCommand = new RelayCommand(ExecuteClose);
        }
        public void LoadTranslationData(TranslationResult result)
        {
            if (result == null) return;

            CurrentWord = result.OriginalText; 
            CurrentTranslatedText = result.TranslatedText ?? string.Empty;
            Phonetic = !string.IsNullOrEmpty(result.Phonetic) ? $"/{result.Phonetic}/" : string.Empty;

            Meanings.Clear();
            if (result.Meanings != null)
            {
                foreach (var m in result.Meanings)
                {
                    Meanings.Add(m);
                }
            }
            IsPinned = false;
            IsSelectionMode = false;
            PrepareWordsForSelection();
        }
        public int CurrentSentenceId { get; set; } = 1;

        private string _currentTranslatedText =
            "Đây là bản dịch thuần tiếng Việt của câu đã chọn.";

        public string CurrentTranslatedText
        {
            get => _currentTranslatedText;
            set
            {
                _currentTranslatedText = value;
                OnPropertyChanged();
            }
        }

        private bool _isPinned;
        public bool IsPinned
        {
            get => _isPinned;
            set
            {
                _isPinned = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SelectableWord> WordList { get; }
            = new ObservableCollection<SelectableWord>();

        private bool _isSelectionMode;
        public bool IsSelectionMode
        {
            get => _isSelectionMode;
            set
            {
                _isSelectionMode = value;
                OnPropertyChanged();
            }
        }

        // ========================
        // COMMANDS
        // ========================
        public ICommand PinCommand { get; }
        public ICommand ReadAloudCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand CloseCommand { get; }

        // ========================
        // PIN LOGIC (TUẦN 3–4)
        // ========================
        private void ExecutePin(object? parameter)
        {
            if (!IsSelectionMode)
            {
                // Lần 1: bật chế độ chọn từ
                PrepareWordsForSelection();
                IsSelectionMode = true;
            }
            else
            {
                // Lần 2: lưu từ đã chọn
                var selectedWords = WordList
                    .Where(w => w.IsSelected)
                    .Select(w => w.Text)
                    .ToList();

                if (selectedWords.Any())
                {
                    _dbService.SavePinnedWords(CurrentSentenceId, selectedWords);
                    IsPinned = true;
                }

                IsSelectionMode = false;
            }
        }

        // ========================
        // TÁCH TỪ
        // ========================
        private void PrepareWordsForSelection()
        {
            WordList.Clear();

            if (string.IsNullOrWhiteSpace(CurrentTranslatedText))
                return;

            var words = CurrentTranslatedText.Split(' ');

            foreach (var word in words)
            {
                WordList.Add(new SelectableWord(word));
            }
        }

        // ========================
        // READ ALOUD
        // ========================
        private void ExecuteReadAloud(object? parameter)
        {
            string textToRead = parameter as string ?? CurrentTranslatedText;

            if (!string.IsNullOrWhiteSpace(textToRead))
            {
                _ttsService.ReadText(textToRead);
            }
        }

        private void ExecuteSettings(object? parameter)
        {
            // Mở cửa sổ settings (Person khác xử lý)
        }

        private void ExecuteClose(object? parameter)
        {
            IsSelectionMode = false;
            WordList.Clear();
        }

        // ========================
        // INotifyPropertyChanged
        // ========================
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this, new PropertyChangedEventArgs(propertyName));
        }
        public class PopupViewModels : BaseViewModel
        {
            private string _displayText;

            public string DisplayText
            {
                get => _displayText;
                set
                {
                    _displayText = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
