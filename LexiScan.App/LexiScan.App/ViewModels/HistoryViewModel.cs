// File: ViewModels/HistoryViewModel.cs
using LexiScan.Core;
using LexiScan.App.Commands;
using LexiScanData.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq; // Cần thiết để xóa một phần tử cụ thể
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LexiScan.App.ViewModels
{
    // Kế thừa từ BaseViewModel
    public class HistoryViewModel : BaseViewModel
    {
        private readonly DatabaseServices _dbService;
        public class HistoryEntry
        {
            public string Id { get; set; }
            public string SearchTerm { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public string DisplayTime => Timestamp.ToString("HH:mm - dd/MM/yyyy");
        }

        // ObservableCollection tự động thông báo khi thêm/xóa/sắp xếp item
        public ObservableCollection<HistoryEntry> HistoryEntries { get; set; }
        public ICommand ClearHistoryCommand { get; } 
        public ICommand DeleteHistoryEntryCommand { get; } 

        public HistoryViewModel()
        {
            HistoryEntries = new ObservableCollection<HistoryEntry>();

            string uid = SessionManager.CurrentUserId;
            if (!string.IsNullOrEmpty(uid))
            {
                _dbService = new DatabaseServices(uid);

                LoadFirebaseHistory();
            }
            
            ClearHistoryCommand = new RelayCommand(ExecuteClearHistory);
            DeleteHistoryEntryCommand = new RelayCommand(ExecuteDeleteHistoryEntry);

        }
        private async void LoadFirebaseHistory()
        {
            if (_dbService == null) return;

            try
            {
                var listFromServer = await _dbService.GetHistoryAsync();

                HistoryEntries.Clear();
                foreach (var item in listFromServer)
                {
                    HistoryEntries.Add(new HistoryEntry
                    {
                        Id = item.SentenceId, // Lấy ID Firebase
                        SearchTerm = item.SourceText,
                        Timestamp = item.CreatedDate
                    });
                }
            }
            catch {  }
        }
        
        private void ExecuteClearHistory(object? _)
        {
            HistoryEntries.Clear();
        }

        // Khắc phục cảnh báo unused parameter và sử dụng object?
        private async void ExecuteDeleteHistoryEntry(object? parameter)
        {
            if (parameter is HistoryEntry entry)
            {
                // 1. Xóa trên giao diện trước cho nhanh
                HistoryEntries.Remove(entry);

                // 2. Xóa trên Firebase
                if (_dbService != null && !string.IsNullOrEmpty(entry.Id))
                {
                    await _dbService.DeleteHistoryAsync(entry.SearchTerm);
                }
            }
        }
    }
}