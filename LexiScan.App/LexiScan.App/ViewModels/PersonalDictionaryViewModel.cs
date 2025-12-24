using LexiScan.Core;
using LexiScan.Core.Models;
using LexiScan.App.Commands;
using LexiScanData.Models;
using LexiScanData.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace LexiScan.App.ViewModels
{
    public class PersonalDictionaryViewModel : BaseViewModel
    {
        private readonly DatabaseServices? _dbService;
        public ObservableCollection<WordExample> SavedWords { get; set; } = new();

        public ICommand LoadDataCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand SearchCommand { get; }

        private string _searchTerm;
        public string SearchTerm { get => _searchTerm; set { _searchTerm = value; OnPropertyChanged(); FilterList(); } }

        // Danh sách gốc để lọc tìm kiếm
        private List<WordExample> _allWords = new();

        public PersonalDictionaryViewModel()
        {
            // Kết nối Database
            string uid = SessionManager.CurrentUserId;
            if (!string.IsNullOrEmpty(uid)) _dbService = new DatabaseServices(uid);

            DeleteItemCommand = new RelayCommand(ExecuteDeleteItem);
            ClearAllCommand = new RelayCommand(ExecuteClearAll);

            // Tải dữ liệu ngay khi mở
            LoadData();
        }

        public async void LoadData()
        {
            if (_dbService == null) return;
            try
            {
                var list = await _dbService.GetSavedItemsAsync();
                _allWords = list; // Lưu vào danh sách gốc

                // Hiển thị ra màn hình
                SavedWords.Clear();
                foreach (var item in list) SavedWords.Add(item);
            }
            catch { }
        }

        private void FilterList()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                SavedWords.Clear();
                foreach (var item in _allWords) SavedWords.Add(item);
            }
            else
            {
                var filtered = _allWords.Where(x => x.SourceText.ToLower().Contains(SearchTerm.ToLower())).ToList();
                SavedWords.Clear();
                foreach (var item in filtered) SavedWords.Add(item);
            }
        }

        private async void ExecuteDeleteItem(object? parameter)
        {
            if (parameter is WordExample item && _dbService != null)
            {
                SavedWords.Remove(item); // Xóa trên giao diện
                _allWords.Remove(item);

                // Xóa trên Firebase (Cần biến Id ở Bước 1)
                if (!string.IsNullOrEmpty(item.Id))
                {
                    await _dbService.DeleteSavedItemAsync(item.Id);
                }
            }
        }

        private async void ExecuteClearAll(object? obj)
        {
            if (_dbService == null || SavedWords.Count == 0) return;

            var result = MessageBox.Show("Bạn có chắc muốn xóa toàn bộ từ điển không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                // Xóa từng cái (Firebase không hỗ trợ xóa 1 phát hết folder users/{id}/saved trừ khi xóa parent)
                // Cách an toàn là xóa folder "saved"
                // Ở đây ta loop xóa trên UI trước cho nhanh
                var listClone = new List<WordExample>(SavedWords);
                SavedWords.Clear();
                _allWords.Clear();

                foreach (var item in listClone)
                {
                    if (!string.IsNullOrEmpty(item.Id)) await _dbService.DeleteSavedItemAsync(item.Id);
                }
            }
        }
    }
}