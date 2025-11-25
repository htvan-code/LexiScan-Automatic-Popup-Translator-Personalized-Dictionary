using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LexiScan.App.ViewModels
{
    // Giả định bạn có lớp BaseViewModel và RelayCommand
    public class HistoryViewModel : BaseViewModel
    {
        // Model đơn giản cho một mục lịch sử
        public class HistoryEntry
        {
            public string SearchTerm { get; set; }
            public DateTime Timestamp { get; set; }
            public string DisplayTime => Timestamp.ToString("HH:mm - dd/MM/yyyy");
        }

        public ObservableCollection<HistoryEntry> HistoryEntries { get; set; }
        public ICommand ClearHistoryCommand { get; set; }
        public ICommand DeleteHistoryEntryCommand { get; set; }

        public HistoryViewModel()
        {
            // Khởi tạo danh sách và Commands
            HistoryEntries = new ObservableCollection<HistoryEntry>();
            ClearHistoryCommand = new RelayCommand(ExecuteClearHistory);
            DeleteHistoryEntryCommand = new RelayCommand(ExecuteDeleteHistoryEntry);

            // Dữ liệu giả định (Placeholder Data)
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

        private void ExecuteClearHistory(object parameter)
        {
            HistoryEntries.Clear();
        }

        private void ExecuteDeleteHistoryEntry(object parameter)
        {
            if (parameter is HistoryEntry entry)
            {
                HistoryEntries.Remove(entry);
            }
        }
    }
}