using LexiScan.Core;
using LexiScan.App.Commands;
using LexiScanData.Services;
using System;
using System.Collections.Generic; 
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows; 
using System.Text;

namespace LexiScan.App.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        private DatabaseServices? _dbService;
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

        public ObservableCollection<HistoryEntry> HistoryEntries { get; set; } = new ObservableCollection<HistoryEntry>();

        private List<HistoryEntry> _allEntries = new List<HistoryEntry>();

        public ICommand ClearHistoryCommand { get; }
        public ICommand DeleteHistoryEntryCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        private string _searchTerm;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
                FilterList(); 
            }
        }

        public HistoryViewModel(AppCoordinator coordinator)
        {
            _coordinator = coordinator;

            LexiScan.Core.Utils.GlobalEvents.OnPersonalDictionaryUpdated += () =>
            {
                string uid = SessionManager.CurrentUserId;
                string token = SessionManager.CurrentAuthToken;

                if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token))
                {
                    _dbService = new DatabaseServices(uid, token);
                    Application.Current.Dispatcher.Invoke(() => LoadFirebaseHistory());
                }
            };

            string currentUid = SessionManager.CurrentUserId;
            string currentToken = SessionManager.CurrentAuthToken;

            if (!string.IsNullOrEmpty(currentUid) && !string.IsNullOrEmpty(currentToken))
            {
                _dbService = new DatabaseServices(currentUid, currentToken);
                LoadFirebaseHistory();
            }

            ClearHistoryCommand = new RelayCommand(ExecuteClearHistory);
            DeleteHistoryEntryCommand = new RelayCommand(ExecuteDeleteHistoryEntry);
            ViewDetailsCommand = new RelayCommand(ExecuteViewDetails);
        }

        //Hàm chuẩn hóa chữ
        private string NormalizeText(string? input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Normalize(NormalizationForm.FormC);
        }

        // --- 1. LOGIC TẢI DỮ LIỆU & LỌC ---
        public async void LoadFirebaseHistory()
        {
            if (_dbService == null)
            {
                string uid = SessionManager.CurrentUserId;
                string token = SessionManager.CurrentAuthToken;
                if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token)) _dbService = new DatabaseServices(uid, SessionManager.CurrentAuthToken);
                else return;
            }

            try
            {
                var listFromServer = await _dbService.GetHistoryAsync();

                // Lưu vào danh sách gốc 
                _allEntries.Clear();

                if (listFromServer != null)
                {
                    foreach (var item in listFromServer.OrderByDescending(x => x.CreatedDate))
                    {
                        _allEntries.Add(new HistoryEntry
                        {
                            Id = item.SentenceId,
                            SearchTerm = NormalizeText(item.SourceText),
                            Timestamp = item.CreatedDate
                        });
                    }
                }

                FilterList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải lịch sử: {ex.Message}");
            }
        }

        private void FilterList()
        {
            HistoryEntries.Clear();

            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                foreach (var item in _allEntries) HistoryEntries.Add(item);
            }
            else
            {
                var filtered = _allEntries.Where(x => x.SearchTerm.ToLower().Contains(SearchTerm.ToLower())).ToList();
                foreach (var item in filtered) HistoryEntries.Add(item);
            }
        }

        // --- 2. LOGIC XÓA TOÀN BỘ ---
        private async void ExecuteClearHistory(object? _)
        {
            if (_dbService == null || HistoryEntries.Count == 0) return;

            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa toàn bộ lịch sử tra cứu không?",
                                         "Xác nhận",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var listClone = new List<HistoryEntry>(HistoryEntries);

                HistoryEntries.Clear();
                _allEntries.Clear();

                foreach (var item in listClone)
                {
                    if (!string.IsNullOrEmpty(item.SearchTerm))
                    {
                        await _dbService.DeleteHistoryAsync(item.SearchTerm);
                    }
                }
            }
        }

        // --- 3. LOGIC XÓA TỪNG MỤC ---
        private async void ExecuteDeleteHistoryEntry(object? parameter)
        {
            if (parameter is HistoryEntry entry)
            {
                HistoryEntries.Remove(entry);
                _allEntries.Remove(entry);

                if (_dbService != null && !string.IsNullOrEmpty(entry.SearchTerm))
                {
                    await _dbService.DeleteHistoryAsync(entry.SearchTerm);
                }
            }
        }

        // --- 4. LOGIC XEM CHI TIẾT ---
        private async void ExecuteViewDetails(object? parameter)
        {
            if (parameter is HistoryEntry entry)
            {
                entry.IsExpanded = !entry.IsExpanded;

                if (entry.IsExpanded && string.IsNullOrEmpty(entry.DetailInfo))
                {
                    entry.IsLoading = true;
                    entry.DetailInfo = string.Empty;

                    try
                    {
                        var result = await _coordinator.TranslateAndGetResultAsync(entry.SearchTerm);

                        if (result != null)
                        {
                            var sb = new System.Text.StringBuilder();
                            if (!string.IsNullOrEmpty(result.Phonetic)) sb.AppendLine($"/{result.Phonetic}/");

                            if (!string.IsNullOrEmpty(result.TranslatedText))
                            {
                                sb.AppendLine($"➤ {NormalizeText(result.TranslatedText)}");
                                sb.AppendLine();
                            }

                            if (result.Meanings != null && result.Meanings.Count > 0)
                            {
                                foreach (var m in result.Meanings)
                                {
                                    sb.AppendLine($"★ {NormalizeText(m.PartOfSpeech)}");
                                    foreach (var def in m.Definitions)
                                    {
                                        sb.AppendLine($"    • {NormalizeText(def)}");
                                    }
                                    sb.AppendLine();
                                }
                            }
                            entry.DetailInfo = sb.ToString().Trim();
                        }
                        else
                        {
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
    }
}