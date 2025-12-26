using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using LexiScanData.Models;
using LexiScan.Core;

namespace LexiScanData.Services
{
    public class DatabaseServices
    {
        // Link Database của bạn
        private const string DbUrl = "https://lexiscan-authentication-default-rtdb.asia-southeast1.firebasedatabase.app/";

        private readonly FirebaseClient _client;
        private readonly string _userId;

        public DatabaseServices(string userId, string authToken)
        {
            _userId = userId;

            _client = new FirebaseClient(DbUrl, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(LexiScan.Core.SessionManager.CurrentAuthToken)
            });
        }

        // ==========================================
        // PHẦN 1: QUẢN LÝ LỊCH SỬ DỊCH (HISTORY)
        // ==========================================

        // 1.1 Thêm vào lịch sử 
        public async Task AddHistoryAsync(Sentences sentence)
        {
            await _client
                .Child("users")
                .Child(_userId)
                .Child("history")
                .PostAsync(sentence);
        }

        // 1.2 Lấy danh sách lịch sử về
        public async Task<List<Sentences>> GetHistoryAsync()
        {
            var items = await _client
                .Child("users")
                .Child(_userId)
                .Child("history")
                .OrderByKey()
                .LimitToLast(50) 
                .OnceAsync<Sentences>();

            return items.Select(i =>
            {
                var s = i.Object;
                s.SentenceId = i.Key; 
                return s;
            }).Reverse().ToList(); 
        }

        // 1.3 Xóa lịch sử theo từ gốc
        public async Task DeleteHistoryAsync(string sourceTextWord)
        {
            var historyNode = _client.Child("users").Child(_userId).Child("history");
            var items = await historyNode.OnceAsync<Sentences>();

            var itemsToDelete = items
                .Where(i => i.Object.SourceText.Equals(sourceTextWord, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var item in itemsToDelete)
            {
                await historyNode.Child(item.Key).DeleteAsync();
            }
        }

        // 1.4 Ghim / Bỏ ghim trong lịch sử
        public async Task<bool> TogglePinStatusAsync(string sentenceId)
        {
            try
            {
                var targetNode = _client.Child("users").Child(_userId).Child("history").Child(sentenceId);
                var sentence = await targetNode.OnceSingleAsync<Sentences>();

                if (sentence != null)
                {
                    bool newStatus = !sentence.IsPinned;
                    await targetNode.PatchAsync(new { IsPinned = newStatus });
                    return newStatus;
                }
                return false;
            }
            catch { return false; }
        }

        // ==========================================
        // PHẦN 2: QUẢN LÝ TỪ ĐÃ LƯU (SAVED VOCABULARY)
        // ==========================================
        public async Task<string?> FindSavedKeyAsync(string textToCheck)
        {
            try
            {
                var items = await _client
                    .Child("users")
                    .Child(_userId)
                    .Child("saved")
                    .OnceAsync<WordExample>();

                // Quét xem có cái nào SourceText trùng khớp không (không phân biệt hoa thường)
                var foundItem = items.FirstOrDefault(i =>
                    i.Object.SourceText.Equals(textToCheck, StringComparison.OrdinalIgnoreCase));

                return foundItem?.Key;
            }
            catch
            {
                return null;
            }
        }

        //Xóa một mục khỏi danh sách 'saved' (Dùng khi Bỏ ghim trên Popup)
        public async Task DeleteSavedItemAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            await _client
                .Child("users")
                .Child(_userId)
                .Child("saved")
                .Child(key)
                .DeleteAsync();
        }
        public async Task SaveSimpleVocabularyAsync(string text, string meaning)
        {
            // Tạo object dữ liệu
            var item = new WordExample
            {
                SourceText = text,      
                Meaning = meaning,      
                SavedDate = DateTime.Now
            };

            // Lưu vào nhánh: users/{uid}/saved
            await _client
                .Child("users")
                .Child(_userId)
                .Child("saved")
                .PostAsync(item);
        }

        // 2.2 Lấy danh sách từ đã lưu (Nếu bạn làm màn hình xem từ vựng)
        public async Task<List<WordExample>> GetSavedItemsAsync()
        {
            var items = await _client
               .Child("users")
               .Child(_userId)
               .Child("saved")
               .OnceAsync<WordExample>();

            return items.Select(i =>
            {
                var w = i.Object;
                w.Id = i.Key; 
                return w;
            }).Reverse().ToList();
        }

        // ==========================================
        // PHẦN 3: TIỆN ÍCH KHÁC
        // ==========================================

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await _client.Child("test_connection").OnceSingleAsync<object>();
                return true;
            }
            catch { return false; }
        }
    }
}