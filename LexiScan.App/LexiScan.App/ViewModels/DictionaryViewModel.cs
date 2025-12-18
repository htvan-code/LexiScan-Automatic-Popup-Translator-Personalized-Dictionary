using System.Collections.ObjectModel;
using System.Windows.Input;
using LexiScan.App.Commands;
using LexiScan.Core;
using LexiScan.Core.Models;

namespace LexiScan.App.ViewModels
{
    public class DictionaryViewModel : BaseViewModel
    {
        private readonly AppCoordinator _coordinator;
        private string _searchText;
        private string _displayWord; // Dùng biến này để hiện ở khung kết quả (tránh nhảy chữ khi đang gõ)
        private string _definitionText;
        private string _phoneticText;
        private string _selectedSuggestion;

        public DictionaryViewModel(AppCoordinator coordinator)
        {
            _coordinator = coordinator;

            // Đăng ký nhận kết quả dịch
            _coordinator.TranslationCompleted += OnTranslationResultReceived;

            SuggestionList = new ObservableCollection<string>();

            // Lệnh khi nhấn Enter hoặc nút Kính lúp
            SearchCommand = new RelayCommand(async (o) =>
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return;

                SuggestionList.Clear(); // Xóa list để ẩn giao diện gợi ý
                await _coordinator.ExecuteSearchAsync(SearchText);
            });

            StartVoiceSearchCommand = new RelayCommand((o) => _coordinator.StartVoiceSearch());

            // Đọc từ đang hiển thị kết quả
            SpeakResultCommand = new RelayCommand((o) => _coordinator.Speak(DisplayWord, 1.0, "US"));
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

                    // Logic: Có chữ -> Load gợi ý. Không chữ -> Xóa gợi ý.
                    if (!string.IsNullOrWhiteSpace(_searchText))
                    {
                        LoadSuggestions(_searchText);
                    }
                    else
                    {
                        SuggestionList.Clear();
                    }
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

                // Khi người dùng click chọn từ gợi ý
                if (!string.IsNullOrEmpty(value))
                {
                    SearchText = value;
                    SuggestionList.Clear(); // Ẩn dropdown ngay lập tức
                    _coordinator.ExecuteSearchAsync(value); // Tìm nghĩa luôn
                }
            }
        }

        // Biến hiển thị tên từ vựng ở khung kết quả dưới
        public string DisplayWord
        {
            get => _displayWord;
            set { _displayWord = value; OnPropertyChanged(); }
        }

        public string DefinitionText
        {
            get => _definitionText;
            set { _definitionText = value; OnPropertyChanged(); }
        }

        public string PhoneticText
        {
            get => _phoneticText;
            set { _phoneticText = value; OnPropertyChanged(); }
        }

        public ICommand SearchCommand { get; }
        public ICommand StartVoiceSearchCommand { get; }
        public ICommand SpeakResultCommand { get; }

        private async void LoadSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                SuggestionList.Clear();
                return;
            }

            // Gọi API lấy gợi ý
            var suggestions = await _coordinator.GetRecommendWordsAsync(query);

            // Cập nhật UI (Bắt buộc dùng Dispatcher để an toàn luồng)
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                SuggestionList.Clear();
                foreach (var s in suggestions) SuggestionList.Add(s);
            });
        }

        private void OnTranslationResultReceived(TranslationResult result)
        {
            // Cập nhật dữ liệu để View hiển thị
            DisplayWord = result.OriginalText;
            DefinitionText = result.TranslatedText;
            PhoneticText = result.Phonetic;
        }
    }
}