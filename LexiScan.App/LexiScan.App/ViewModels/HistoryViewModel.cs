using LexiScan.Core;
using LexiScan.App.Commands;
using LexiScanData.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq; // Cần dòng này để dùng OrderByDescending
using System.Windows.Input;
using System.Threading.Tasks;

namespace LexiScan.App.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        private DatabaseServices _dbService;
        private readonly AppCoordinator _coordinator;

        public class HistoryEntry : BaseViewModel
        {
            public string Id { get; set; }
            public string SearchTerm { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public string DisplayTime => Timestamp.ToString("HH:mm - dd/MM/yyyy");

            private bool _isExpanded;
            public bool IsExpanded
            {
                get => _isExpanded;
                set { _isExpanded = value; OnPropertyChanged(); }
            }

            private bool _isLoading;
            public bool IsLoading
            {
                get => _isLoading;
                set { _isLoading = value; OnPropertyChanged(); }
            }

            private string _detailInfo;
            public string DetailInfo
            {
                get => _detailInfo;
                set { _detailInfo = value; OnPropertyChanged(); }
            }
        }

        public ObservableCollection<HistoryEntry> HistoryEntries { get; set; }
        public ICommand ClearHistoryCommand { get; }
        public ICommand DeleteHistoryEntryCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        public HistoryViewModel(AppCoordinator coordinator)
        {
            _coordinator = coordinator;
            HistoryEntries = new ObservableCollection<HistoryEntry>();

            string uid = SessionManager.CurrentUserId;
            if (!string.IsNullOrEmpty(uid))
            {
                _dbService = new DatabaseServices(uid);
                LoadFirebaseHistory();
            }

            ClearHistoryCommand = new RelayCommand(ExecuteClearHistory);
            DeleteHistoryEntryCommand = new RelayCommand(ExecuteDeleteHistoryEntry);
            ViewDetailsCommand = new RelayCommand(ExecuteViewDetails);
        }

        // --- LOGIC XỬ LÝ KHI BẤM NÚT "XEM CHI TIẾT" ---
        private async void ExecuteViewDetails(object? parameter)
        {
            if (parameter is HistoryEntry entry)
            {
                // 1. Đảo ngược trạng thái (Đóng <-> Mở)
                entry.IsExpanded = !entry.IsExpanded;

                // 2. Chỉ tải dữ liệu nếu Đang Mở và Chưa có dữ liệu
                if (entry.IsExpanded && string.IsNullOrEmpty(entry.DetailInfo))
                {
                    entry.IsLoading = true;
                    entry.DetailInfo = string.Empty;

                    try
                    {
                        // [QUAN TRỌNG] Gọi hàm dịch để lấy kết quả đầy đủ (TranslationResult)
                        // Lưu ý: Bạn cần đảm bảo AppCoordinator có hàm trả về kết quả TranslationResult
                        // Nếu chưa có, bạn có thể dùng hàm GetTranslationResultAsync (xem hướng dẫn bên dưới)

                        // Giả sử coordinator có hàm TranslateAndGetResultAsync trả về TranslationResult
                        // Nếu bạn chưa có hàm này, hãy xem phần "Lưu ý" ở cuối bài để thêm vào AppCoordinator
                        var result = await _coordinator.TranslateAndGetResultAsync(entry.SearchTerm);

                        if (result != null)
                        {
                            var sb = new System.Text.StringBuilder();

                            // 1. Phiên âm (Ví dụ: /'ɔ:də/)
                            if (!string.IsNullOrEmpty(result.Phonetic))
                            {
                                sb.AppendLine($"/{result.Phonetic}/");
                            }

                            // 2. Nghĩa chính (Ví dụ: đặt hàng)
                            if (!string.IsNullOrEmpty(result.TranslatedText))
                            {
                                // Thêm dấu gạch ngang hoặc định dạng để làm nổi bật
                                sb.AppendLine($"➤ {result.TranslatedText}");
                                sb.AppendLine(); // Xuống dòng
                            }

                            // 3. Các loại từ và định nghĩa (Noun, Verb...)
                            if (result.Meanings != null && result.Meanings.Count > 0)
                            {
                                foreach (var m in result.Meanings)
                                {
                                    // Loại từ (Noun, Verb...)
                                    sb.AppendLine($"★ {m.PartOfSpeech}");

                                    // Các định nghĩa con
                                    foreach (var def in m.Definitions)
                                    {
                                        sb.AppendLine($"    • {def}");
                                    }
                                    sb.AppendLine(); // Cách dòng giữa các loại từ
                                }
                            }

                            // Gán kết quả đã format vào DetailInfo
                            entry.DetailInfo = sb.ToString().Trim();
                        }
                        else
                        {
                            // Trường hợp không tìm thấy (Fall-back gọi gợi ý)
                            var suggestions = await _coordinator.GetRecommendWordsAsync(entry.SearchTerm);
                            if (suggestions.Count > 0)
                                entry.DetailInfo = $"Gợi ý: {string.Join(", ", suggestions)}";
                            else
                                entry.DetailInfo = "Không tìm thấy định nghĩa chi tiết.";
                        }
                    }
                    catch (Exception ex)
                    {
                        entry.DetailInfo = $"Lỗi tải dữ liệu: {ex.Message}";
                    }
                    finally
                    {
                        entry.IsLoading = false;
                    }
                }
            }
        }

        public async void LoadFirebaseHistory()
        {
            if (_dbService == null)
            {
                string uid = SessionManager.CurrentUserId;
                if (!string.IsNullOrEmpty(uid))
                {
                    _dbService = new DatabaseServices(uid);
                }
                else
                {
                    return;
                }
            }

            try
            {
                var listFromServer = await _dbService.GetHistoryAsync();

                HistoryEntries.Clear();

                if (listFromServer != null)
                {
                    // [SỬA ĐỔI QUAN TRỌNG] 
                    // Sắp xếp giảm dần theo ngày tạo (CreatedDate) để đảm bảo cái mới nhất luôn lên đầu
                    var sortedList = listFromServer.OrderByDescending(x => x.CreatedDate);

                    foreach (var item in sortedList)
                    {
                        HistoryEntries.Add(new HistoryEntry
                        {
                            Id = item.SentenceId,
                            SearchTerm = item.SourceText,
                            Timestamp = item.CreatedDate
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải lịch sử: {ex.Message}");
            }
        }

        private void ExecuteClearHistory(object? _)
        {
            HistoryEntries.Clear();
            // Thêm logic gọi API xóa tất cả nếu cần
        }

        private async void ExecuteDeleteHistoryEntry(object? parameter)
        {
            if (parameter is HistoryEntry entry)
            {
                HistoryEntries.Remove(entry);
                if (_dbService != null && !string.IsNullOrEmpty(entry.SearchTerm))
                {
                    await _dbService.DeleteHistoryAsync(entry.SearchTerm);
                }
            }
        }
    }
}