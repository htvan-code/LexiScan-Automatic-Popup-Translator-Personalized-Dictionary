// File: ViewModels/HistoryViewModel.cs

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using LexiScan.App.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq; // Cần thiết để xóa một phần tử cụ thể

namespace LexiScan.App.ViewModels
{
    // Kế thừa từ BaseViewModel
    public class HistoryViewModel : BaseViewModel
    {
        // Model đơn giản cho một mục lịch sử
        public class HistoryEntry
        {
            // Khắc phục CS8618: Khởi tạo với string.Empty
            public string SearchTerm { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public string DisplayTime => Timestamp.ToString("HH:mm - dd/MM/yyyy");
        }

        // ObservableCollection tự động thông báo khi thêm/xóa/sắp xếp item
        public ObservableCollection<HistoryEntry> HistoryEntries { get; set; }
        public ICommand ClearHistoryCommand { get; } // Dùng `get;` để tuân thủ ReadOnly
        public ICommand DeleteHistoryEntryCommand { get; } // Dùng `get;` để tuân thủ ReadOnly

        public HistoryViewModel()
        {
            HistoryEntries = new ObservableCollection<HistoryEntry>();

            // Khắc phục CS0246: RelayCommand đã được tham chiếu đúng
            ClearHistoryCommand = new RelayCommand(ExecuteClearHistory);
            // DeleteHistoryEntryCommand chấp nhận tham số (HistoryEntry)
            DeleteHistoryEntryCommand = new RelayCommand(ExecuteDeleteHistoryEntry);

            LoadPlaceholderData();
        }

        private void LoadPlaceholderData()
        {
            // Thêm một số dữ liệu mẫu để kiểm tra giao diện
            HistoryEntries.Add(new HistoryEntry { SearchTerm = "mitosis", Timestamp = DateTime.Now.AddHours(-1) });
            HistoryEntries.Add(new HistoryEntry { SearchTerm = "paradigm shift", Timestamp = DateTime.Now.AddHours(-5) });
            HistoryEntries.Add(new HistoryEntry { SearchTerm = "infrastructure", Timestamp = DateTime.Now.AddDays(-1) });
            HistoryEntries.Add(new HistoryEntry { SearchTerm = "commitment", Timestamp = DateTime.Now.AddDays(-2) });
        }

        // Khắc phục cảnh báo unused parameter bằng cách sử dụng dấu gạch dưới `_`
        private void ExecuteClearHistory(object? _)
        {
            HistoryEntries.Clear();
        }

        // Khắc phục cảnh báo unused parameter và sử dụng object?
        private void ExecuteDeleteHistoryEntry(object? parameter)
        {
            if (parameter is HistoryEntry entry)
            {
                HistoryEntries.Remove(entry);
            }
        }
    }
}