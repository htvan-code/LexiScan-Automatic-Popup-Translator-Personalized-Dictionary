using LexiScan.Core;
using LexiScan.Core.Models;
using LexiScan.App.Commands;
using LexiScanData.Models;
using LexiScanData.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LexiScan.App.ViewModels
{
    public class PersonalDictionaryViewModel : BaseViewModel
    {
        private readonly DatabaseServices? _dbService;
        private readonly AppCoordinator _coordinator; // Cần để tra nghĩa chi tiết

        // --- [CLASS WRAPPER CHO ITEM ĐỂ HỖ TRỢ ĐÓNG/MỞ] ---
        public class PersonalWordEntry : BaseViewModel
        {
            public string Id { get; set; }
            public string SourceText { get; set; } = string.Empty;
            public DateTime SavedDate { get; set; }

            // Biến trạng thái UI
            private bool _isExpanded;
            public bool IsExpanded { get => _isExpanded; set { _isExpanded = value; OnPropertyChanged(); } }

            private bool _isLoading;
            public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }

            private string _detailInfo;
            public string DetailInfo { get => _detailInfo; set { _detailInfo = value; OnPropertyChanged(); } }
        }

        // Đổi từ WordExample sang PersonalWordEntry
        public ObservableCollection<PersonalWordEntry> SavedWords { get; set; } = new();

        public ICommand LoadDataCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand ViewDetailsCommand { get; } // Command Xem Chi Tiết

        private string _searchTerm;
        public string SearchTerm { get => _searchTerm; set { _searchTerm = value; OnPropertyChanged(); FilterList(); } }

        // Danh sách gốc (cũng phải đổi sang PersonalWordEntry)
        private List<PersonalWordEntry> _allEntries = new();

        // [QUAN TRỌNG] Cần thêm AppCoordinator vào Constructor
        public PersonalDictionaryViewModel(AppCoordinator coordinator)
        {
            _coordinator = coordinator;

            string uid = SessionManager.CurrentUserId;
            if (!string.IsNullOrEmpty(uid)) _dbService = new DatabaseServices(uid);

            DeleteItemCommand = new RelayCommand(ExecuteDeleteItem);
            ClearAllCommand = new RelayCommand(ExecuteClearAll);
            ViewDetailsCommand = new RelayCommand(ExecuteViewDetails);

            LoadData();
        }

        // --- LOGIC XEM CHI TIẾT (Giống HistoryViewModel) ---
        private async void ExecuteViewDetails(object? parameter)
        {
            if (parameter is PersonalWordEntry entry)
            {
                entry.IsExpanded = !entry.IsExpanded;

                if (entry.IsExpanded && string.IsNullOrEmpty(entry.DetailInfo))
                {
                    entry.IsLoading = true;
                    entry.DetailInfo = string.Empty;

                    try
                    {
                        // Gọi Coordinator để lấy thông tin đầy đủ
                        var result = await _coordinator.TranslateAndGetResultAsync(entry.SourceText);

                        if (result != null)
                        {
                            var sb = new System.Text.StringBuilder();
                            if (!string.IsNullOrEmpty(result.Phonetic)) sb.AppendLine($"/{result.Phonetic}/");
                            if (!string.IsNullOrEmpty(result.TranslatedText))
                            {
                                sb.AppendLine($"➤ {result.TranslatedText}");
                                sb.AppendLine();
                            }

                            if (result.Meanings != null && result.Meanings.Count > 0)
                            {
                                foreach (var m in result.Meanings)
                                {
                                    sb.AppendLine($"★ {m.PartOfSpeech}");
                                    foreach (var def in m.Definitions) sb.AppendLine($"    • {def}");
                                    sb.AppendLine();
                                }
                            }
                            entry.DetailInfo = sb.ToString().Trim();
                        }
                        else
                        {
                            entry.DetailInfo = "Không tìm thấy dữ liệu chi tiết.";
                        }
                    }
                    catch (Exception ex)
                    {
                        entry.DetailInfo = $"Lỗi: {ex.Message}";
                    }
                    finally
                    {
                        entry.IsLoading = false;
                    }
                }
            }
        }

        public async void LoadData()
        {
            if (_dbService == null) return;
            try
            {
                var list = await _dbService.GetSavedItemsAsync();

                _allEntries.Clear();
                // Chuyển đổi WordExample -> PersonalWordEntry
                // Sắp xếp ngày mới nhất lên đầu (OrderByDescending)
                foreach (var item in list.OrderByDescending(x => x.SavedDate))
                {
                    _allEntries.Add(new PersonalWordEntry
                    {
                        Id = item.Id,
                        SourceText = item.SourceText,
                        SavedDate = item.SavedDate
                    });
                }

                FilterList(); // Hiển thị ra UI
            }
            catch { }
        }

        private void FilterList()
        {
            SavedWords.Clear();
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                foreach (var item in _allEntries) SavedWords.Add(item);
            }
            else
            {
                var filtered = _allEntries.Where(x => x.SourceText.ToLower().Contains(SearchTerm.ToLower())).ToList();
                foreach (var item in filtered) SavedWords.Add(item);
            }
        }

        private async void ExecuteDeleteItem(object? parameter)
        {
            if (parameter is PersonalWordEntry entry && _dbService != null)
            {
                SavedWords.Remove(entry);
                _allEntries.Remove(entry);

                if (!string.IsNullOrEmpty(entry.Id))
                {
                    await _dbService.DeleteSavedItemAsync(entry.Id);
                }
            }
        }

        private async void ExecuteClearAll(object? obj)
        {
            if (_dbService == null || SavedWords.Count == 0) return;

            var result = MessageBox.Show("Bạn có chắc muốn xóa toàn bộ từ điển không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                var listClone = new List<PersonalWordEntry>(SavedWords);
                SavedWords.Clear();
                _allEntries.Clear();

                foreach (var item in listClone)
                {
                    if (!string.IsNullOrEmpty(item.Id)) await _dbService.DeleteSavedItemAsync(item.Id);
                }
            }
        }
    }
}