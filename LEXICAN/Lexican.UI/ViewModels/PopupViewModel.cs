// Project: LexiScan.UI
// File: ViewModels/PopupViewModel.cs
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace LexiScan.UI.ViewModels;
public class PopupViewModel : INotifyPropertyChanged
{
    // Khởi tạo các dịch vụ
    private readonly DatabaseService _dbService = new DatabaseService();
    private readonly TtsService _ttsService = new TtsService();

    // Thuộc tính Data Binding
    public int CurrentSentenceId { get; set; } = 1; // Giả định ID

    // Giả định Text
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

    // ICommand Properties (Nhiệm vụ Tuần 2) [cite: 57]
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

    // Logic cho Pin Command (gọi Database Service)
    private void ExecutePin(object parameter)
    {
        // Gọi TogglePinStatus(sentenceId) [cite: 56]
        IsPinned = _dbService.TogglePinStatus(CurrentSentenceId);
    }

    // Logic cho Read Aloud Command (gọi TTS Service của Person 2) [cite: 36]
    private void ExecuteReadAloud(object parameter)
    {
        string textToRead = parameter as string ?? CurrentTranslatedText;
        if (!string.IsNullOrEmpty(textToRead))
        {
            _ttsService.ReadText(textToRead);
        }
    }

    // Logic cho Settings và Close (sẽ hoàn thiện sau) [cite: 34]
    private void ExecuteSettings(object parameter) { /* Mở cửa sổ chính (Tuần 5-6) */ }
    private void ExecuteClose(object parameter) { /* Đóng Popup */ }

    // Triển khai INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}