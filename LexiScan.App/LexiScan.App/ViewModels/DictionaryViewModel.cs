using System.Collections.ObjectModel; // [CẦN THÊM] Để dùng ObservableCollection
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
        private string _definitionText;
        private string _phoneticText;

        // [MỚI] Biến để xử lý item được chọn trong ListBox gợi ý
        private string _selectedSuggestion;

        public DictionaryViewModel(AppCoordinator coordinator)
        {
            _coordinator = coordinator;
            _coordinator.VoiceSearchCompleted += OnVoiceSearchCompleted;
            _coordinator.TranslationCompleted += OnTranslationResultReceived;

            // Khởi tạo danh sách gợi ý rỗng
            SuggestionList = new ObservableCollection<string>();

            // Command cho nút Enter hoặc nút Kính lúp
            SearchCommand = new RelayCommand(async (o) =>
            {
                // Khi bấm Enter/Search -> Ẩn gợi ý và tìm nghĩa
                SuggestionList.Clear();
                await _coordinator.ExecuteSearchAsync(SearchText);
            });

            StartVoiceSearchCommand = new RelayCommand((o) => _coordinator.StartVoiceSearch());
            SpeakResultCommand = new RelayCommand((o) => _coordinator.Speak(SearchText, 1.0, "US"));
        }

        // [MỚI] Danh sách gợi ý binding ra ListBox
        public ObservableCollection<string> SuggestionList { get; set; }

        // Trong DictionaryViewModel.cs
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();

                    // [QUAN TRỌNG] Gọi hàm load gợi ý ngay khi gõ phím
                    LoadSuggestions(_searchText);
                }
            }
        }

        // [LOGIC MỚI] Khi người dùng click chọn 1 dòng trong ListBox
        public string SelectedSuggestion
        {
            get => _selectedSuggestion;
            set
            {
                _selectedSuggestion = value;
                OnPropertyChanged();

                if (!string.IsNullOrEmpty(_selectedSuggestion))
                {
                    // 1. Điền từ đã chọn vào ô tìm kiếm
                    _searchText = _selectedSuggestion;
                    OnPropertyChanged(nameof(SearchText));

                    // 2. Xóa danh sách gợi ý để ẩn Popup đi
                    SuggestionList.Clear();

                    // 3. Bây giờ mới thực sự gọi hàm tìm nghĩa (hiện chi tiết bên dưới)
                    _coordinator.ExecuteSearchAsync(_selectedSuggestion);
                }
            }
        }

        // Hàm gọi API lấy gợi ý (Google Suggest)
        private async void LoadSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                SuggestionList.Clear();
                return;
            }

            // Gọi qua AppCoordinator (bạn đã có hàm GetRecommendWordsAsync trong AppCoordinator ở các bước trước)
            var suggestions = await _coordinator.GetRecommendWordsAsync(query);

            SuggestionList.Clear();
            foreach (var item in suggestions)
            {
                SuggestionList.Add(item);
            }
        }

        // ... (Các phần DefinitionText, PhoneticText, Commands giữ nguyên) ...

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

        private void OnVoiceSearchCompleted(string text)
        {
            SearchText = text;
            _coordinator.ExecuteSearchAsync(text);
        }

        // Nhận kết quả từ Coordinator trả về để hiển thị
        private void OnTranslationResultReceived(TranslationResult result)
        {
            DefinitionText = result.TranslatedText;
            // Giả sử model TranslationResult của bạn có trường Phonetic
            // PhoneticText = result.Phonetic; 
        }
    }
}