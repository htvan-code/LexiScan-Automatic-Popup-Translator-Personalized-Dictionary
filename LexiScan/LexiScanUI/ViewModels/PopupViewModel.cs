using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LexiScanUI.Helpers;

namespace LexiScanUI.ViewModels
{
    public class PopupViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseServices _dbService = new DatabaseServices();
        private readonly TtsService _ttsService = new TtsService();

        public int CurrentSentenceId { get; set; } = 1;

        private string _currentTranslatedText = "Đây là bản dịch thuần tiếng Việt của câu đã chọn.";
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

        // ICommand Properties
        public ICommand PinCommand { get; }
        public ICommand ReadAloudCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand CloseCommand { get; }

        public PopupViewModel()
        {
            // Khởi tạo các lệnh sử dụng RelayCommand
            PinCommand = new RelayCommand(ExecutePin);
            ReadAloudCommand = new RelayCommand(ExecuteReadAloud);
            SettingsCommand = new RelayCommand(ExecuteSettings);
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        // Logic cho Pin Command
        private void ExecutePin(object parameter)
        {
            IsPinned = _dbService.TogglePinStatus(CurrentSentenceId);
        }

        // Logic cho Read Aloud Command
        private void ExecuteReadAloud(object parameter)
        {
            string? textToRead = parameter as string ?? CurrentTranslatedText;
            if (!string.IsNullOrEmpty(textToRead))
            {
                _ttsService.ReadText(textToRead);
            }
        }

        // Logic cho Settings và Close
        private void ExecuteSettings(object parameter) { /* Mở cửa sổ chính (Tuần 5-6) */ }
        private void ExecuteClose(object parameter) { /* Đóng Popup */ }

        // Triển khai INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged; // Sửa lỗi CS8618
        protected void OnPropertyChanged([CallerMemberName] string? name = null) // Thêm ? cho name
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    internal class TtsService
    {
        internal void ReadText(string textToRead)
        {
            throw new NotImplementedException();
        }
    }

    internal class DatabaseServices
    {
        internal bool TogglePinStatus(int currentSentenceId)
        {
            throw new NotImplementedException();
        }
    }
}