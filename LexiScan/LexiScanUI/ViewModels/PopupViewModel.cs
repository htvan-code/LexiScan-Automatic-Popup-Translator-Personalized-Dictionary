using LexiScanData.Services;
using LexiScanService;
using LexiScanUI.Helpers;
using System.Collections.ObjectModel;
using System.Linq; // Cần thêm cái này cho .Where().Select()
using System.Windows.Input;
using LexiScan.Core.Models;

namespace LexiScanUI.ViewModels
{
    // 1. Kế thừa BaseViewModel thay vì INotifyPropertyChanged
    public class PopupViewModel : BaseViewModel
    {
        // ... Các Property giữ nguyên ...
        private string _currentWord;
        public string CurrentWord
        {
            get => _currentWord;
            set { _currentWord = value; OnPropertyChanged(); } // Dùng hàm của BaseViewModel
        }

        private string _phonetic;
        public string Phonetic
        {
            get => _phonetic;
            set { _phonetic = value; OnPropertyChanged(); }
        }

        private string _currentTranslatedText = "Đây là bản dịch thuần tiếng Việt...";
        public string CurrentTranslatedText
        {
            get => _currentTranslatedText;
            set { _currentTranslatedText = value; OnPropertyChanged(); }
        }

        private bool _isPinned;
        public bool IsPinned
        {
            get => _isPinned;
            set { _isPinned = value; OnPropertyChanged(); }
        }

        private bool _isSelectionMode;
        public bool IsSelectionMode
        {
            get => _isSelectionMode;
            set { _isSelectionMode = value; OnPropertyChanged(); }
        }

        // Services & Collections
        private readonly DatabaseServices _dbService;
        private readonly TtsService _ttsService;
        public ObservableCollection<Meaning> Meanings { get; } = new ObservableCollection<Meaning>();
        public ObservableCollection<SelectableWord> WordList { get; } = new ObservableCollection<SelectableWord>();

        public int CurrentSentenceId { get; set; } = 1;

        // Commands
        public ICommand PinCommand { get; }
        public ICommand ReadAloudCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand CloseCommand { get; }

        // Constructor
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

        // ... Giữ nguyên các hàm logic (LoadTranslationData, ExecutePin, v.v.) ...

        public void LoadTranslationData(TranslationResult result)
        {
            if (result == null) return;
            CurrentWord = result.OriginalText;
            CurrentTranslatedText = result.TranslatedText ?? string.Empty;
            Phonetic = !string.IsNullOrEmpty(result.Phonetic) ? $"/{result.Phonetic}/" : string.Empty;

            Meanings.Clear();
            if (result.Meanings != null)
            {
                foreach (var m in result.Meanings) Meanings.Add(m);
            }
            IsPinned = false;
            IsSelectionMode = false;
            PrepareWordsForSelection();
        }

        private void ExecutePin(object? parameter)
        {
            if (!IsSelectionMode)
            {
                PrepareWordsForSelection();
                IsSelectionMode = true;
            }
            else
            {
                var selectedWords = WordList.Where(w => w.IsSelected).Select(w => w.Text).ToList();
                if (selectedWords.Any())
                {
                    _dbService.SavePinnedWords(CurrentSentenceId, selectedWords);
                    IsPinned = true;
                }
                IsSelectionMode = false;
            }
        }

        private void PrepareWordsForSelection()
        {
            WordList.Clear();
            if (string.IsNullOrWhiteSpace(CurrentTranslatedText)) return;
            var words = CurrentTranslatedText.Split(' ');
            foreach (var word in words) WordList.Add(new SelectableWord(word));
        }

        private void ExecuteReadAloud(object? parameter)
        {
            // Ưu tiên đọc từ gốc (tiếng Anh)
            string textToRead = !string.IsNullOrWhiteSpace(CurrentWord)
                ? CurrentWord
                : CurrentTranslatedText;

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

        // ĐÃ XÓA: class PopupViewModels thừa ở dưới đáy
        // ĐÃ XÓA: public event PropertyChangedEventHandler và OnPropertyChanged (vì đã có ở BaseViewModel)
    }
}