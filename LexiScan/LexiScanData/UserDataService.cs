using Firebase.Database;
using Firebase.Database.Query;
using LexiScanData.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LexiScanData.Services
{
    public class UserDataService
    {
        // Thay link Realtime Database của bạn vào đây
        private const string DbUrl = "https://lexiscan-authentication-default-rtdb.asia-southeast1.firebasedatabase.app/";

        private readonly FirebaseClient _client;
        private readonly string _userId; // ID của người dùng hiện tại

        // BẮT BUỘC: Phải truyền UserId vào khi khởi tạo
        public UserDataService(string userId)
        {
            _client = new FirebaseClient(DbUrl);
            _userId = userId;
        }

        // --- 1. LƯU LỊCH SỬ DỊCH (Tự động) ---
        public async Task AddHistoryAsync(Sentences sentence)
        {
            // Lưu vào đường dẫn: users/{uid}/history
            await _client
                .Child("users")
                .Child(_userId)
                .Child("history")
                .PostAsync(sentence);
        }

        public async Task<List<Sentences>> GetHistoryAsync()
        {
            var items = await _client
                .Child("users")
                .Child(_userId)
                .Child("history")
                .OrderByKey()
                .LimitToLast(50) // Lấy 50 cái mới nhất
                .OnceAsync<Sentences>();

            return items.Select(i =>
            {
                var s = i.Object;
                s.SentenceId = i.Key;
                return s;
            }).Reverse().ToList(); // Đảo ngược để cái mới nhất lên đầu
        }

        // --- 2. LƯU CÂU VÀO TỪ (Tính năng Popup) ---
        // wordKey: Từ vựng người dùng chọn (VD: "apple")
        // sentence: Câu ví dụ chứa từ đó
        public async Task SaveSentenceToWordAsync(string wordKey, WordExample example)
        {
            string cleanWord = wordKey.Trim().ToLower(); // Chuẩn hóa từ khóa

            // Lưu vào đường dẫn: users/{uid}/vocab/apple/examples
            await _client
                .Child("users")
                .Child(_userId)
                .Child("vocab")
                .Child(cleanWord)
                .Child("examples")
                .PostAsync(example);
        }

        // --- 3. TRA TỪ ĐỂ XEM CÂU ĐÃ LƯU ---
        public async Task<List<WordExample>> GetExamplesForWordAsync(string wordKey)
        {
            string cleanWord = wordKey.Trim().ToLower();

            var items = await _client
                .Child("users")
                .Child(_userId)
                .Child("vocab")
                .Child(cleanWord)
                .Child("examples")
                .OnceAsync<WordExample>();

            return items.Select(i => i.Object).ToList();
        }
    }
}