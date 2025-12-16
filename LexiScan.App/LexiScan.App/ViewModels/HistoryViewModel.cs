// File: ViewModels/HistoryViewModel.cs

using LexiScan.App.Commands;
using LexiScanData;
using LexiScanData.Models;
// --- DÒNG BẠN ĐÃ THÊM ---
using LexiScanData.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq; // Để sắp xếp hoặc xóa
using System.Runtime.CompilerServices;
using System.Windows.Input;
// -------------------------

namespace LexiScan.App.ViewModels
{
    // CẦN XÓA HOẶC BỎ QUA CLASS HistoryEntry NỘI BỘ NÀY
    // public class HistoryEntry { ... } 

    // Kế thừa từ BaseViewModel
    public class HistoryViewModel : BaseViewModel
    {
        // 1. KHAI BÁO SERVICE (Đấu nối với project LexiScanData/P4)
        private readonly DatabaseServices _dbService;

        // 2. DÙNG MODEL THẬT (Word) CHO COLLECTION
        public ObservableCollection<Word> HistoryEntries { get; set; } // <<<<<<<< Dùng Word
        public ICommand ClearHistoryCommand { get; }
        public ICommand DeleteHistoryEntryCommand { get; }

        public HistoryViewModel()
        {
            // 3. KHỞI TẠO SERVICE
            _dbService = new DatabaseServices();
            HistoryEntries = new ObservableCollection<Word>();

            ClearHistoryCommand = new RelayCommand(ExecuteClearHistory);
            // DeleteHistoryEntryCommand chấp nhận tham số (Word)
            DeleteHistoryEntryCommand = new RelayCommand(ExecuteDeleteHistoryEntry);

            // 4. GỌI HÀM LOAD THẬT
            LoadHistoryFromDb();
        }

        // 4.1 HÀM LOAD DỮ LIỆU THẬT TỪ DB
        private void LoadHistoryFromDb()
        {
            // Giả sử P4 có hàm GetAllWords() trong Service
            var wordsFromDb = _dbService.GetAllWords();

            HistoryEntries.Clear();
            // Thường sắp xếp ngược lại (mới nhất lên đầu)
            foreach (var word in wordsFromDb.OrderByDescending(w => w.WordId))
            {
                HistoryEntries.Add(word);
            }
        }

        // CẦN XÓA LoadPlaceholderData()

        // 5.1 LOGIC XÓA HẾT
        private void ExecuteClearHistory(object? _)
        {
            // BƯỚC 1: XÓA TRONG DATABASE
            _dbService.ClearAllWords();

            // BƯỚC 2: XÓA TRÊN UI
            HistoryEntries.Clear();
        }

        // 5.2 LOGIC XÓA MỘT MỤC
        private void ExecuteDeleteHistoryEntry(object? parameter)
        {
            // Cast tham số sang Word Model thật
            if (parameter is Word word)
            {
                // BƯỚC 1: XÓA TRONG DATABASE
                _dbService.DeleteWord(word.WordId);

                // BƯỚC 2: XÓA TRÊN UI
                HistoryEntries.Remove(word);
            }
        }
    }
}