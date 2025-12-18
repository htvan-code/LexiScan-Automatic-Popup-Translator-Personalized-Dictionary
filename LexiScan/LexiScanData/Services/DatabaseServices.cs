using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using LexiScanData.Models;

namespace LexiScanData.Services
{
    public class DatabaseServices
    {
        // Link Database của bạn
        private const string DbUrl = "https://lexiscan-authentication-default-rtdb.asia-southeast1.firebasedatabase.app/";

        private readonly FirebaseClient _client;
        private readonly string _userId;

        // BẮT BUỘC: Phải có UserId để biết đang thao tác trên tài khoản nào
        public DatabaseServices(string userId)
        {
            _client = new FirebaseClient(DbUrl);
            _userId = userId;
        }

        // 1. GHIM / BỎ GHIM (Toggle Pin)
        // Thay đổi: int sentenceId -> string sentenceId
        public async Task<bool> TogglePinStatusAsync(string sentenceId)
        {
            try
            {
                // Đường dẫn tới thuộc tính IsPinned của câu đó
                var targetNode = _client
                    .Child("users")
                    .Child(_userId)
                    .Child("history")
                    .Child(sentenceId);

                // Lấy câu hiện tại để xem trạng thái cũ
                var sentence = await targetNode.OnceSingleAsync<Sentences>();

                if (sentence != null)
                {
                    bool newStatus = !sentence.IsPinned;

                    // Chỉ update đúng 1 trường IsPinned (dùng Patch để không ghi đè cả object)
                    await targetNode.PatchAsync(new { IsPinned = newStatus });

                    return newStatus;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // 2. XÓA LỊCH SỬ DỰA TRÊN TỪ GỐC
        public async Task DeleteHistoryAsync(string sourceTextWord)
        {
            // Firebase không hỗ trợ "DELETE WHERE..." như SQL.
            // Cách làm: Tải list về -> Lọc -> Xóa từng cái (hoặc dùng Query của Firebase để lọc)

            var historyNode = _client.Child("users").Child(_userId).Child("history");

            // Lấy toàn bộ lịch sử (hoặc lọc sơ bộ)
            var items = await historyNode.OnceAsync<Sentences>();

            // Tìm những node có SourceText trùng khớp
            var itemsToDelete = items
                .Where(i => i.Object.SourceText.Equals(sourceTextWord, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Xóa từng node tìm được
            foreach (var item in itemsToDelete)
            {
                await historyNode.Child(item.Key).DeleteAsync();
            }
        }

        // 3. KIỂM TRA KẾT NỐI (Đơn giản là thử đọc 1 cái gì đó)
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                // Thử đọc data rác, nếu không lỗi Exception nghĩa là kết nối OK
                await _client.Child("test_connection").OnceSingleAsync<object>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 4. LƯU CÂU VÀO CÁC TỪ ĐƯỢC CHỌN (Quan trọng nhất)
        // Thay đổi: Logic Many-to-Many cũ -> Logic Cây thư mục mới
        public async Task SavePinnedWordsAsync(string sentenceId, List<string> selectedWords)
        {
            // B1: Lấy thông tin chi tiết của câu gốc từ History
            var sentenceNode = _client.Child("users").Child(_userId).Child("history").Child(sentenceId);
            var sentenceObj = await sentenceNode.OnceSingleAsync<Sentences>();

            if (sentenceObj == null) return;

            // B2: Cập nhật trạng thái câu gốc thành Đã Ghim (IsPinned = true)
            if (!sentenceObj.IsPinned)
            {
                await sentenceNode.PatchAsync(new { IsPinned = true });
            }

            // B3: Tạo object "Câu ví dụ" để lưu vào danh sách từ vựng
            var exampleData = new WordExample
            {
                OriginalSentence = sentenceObj.SourceText,
                TranslatedMeaning = sentenceObj.TranslatedText,
                SavedDate = DateTime.Now
            };

            // B4: Duyệt qua danh sách từ người dùng chọn và lưu
            foreach (var wordText in selectedWords)
            {
                if (string.IsNullOrWhiteSpace(wordText)) continue;

                string cleanWord = wordText.Trim().ToLower();

                // Lưu vào đường dẫn: users/{uid}/vocab/{từ}/examples/
                // Firebase tự động tạo nhánh "từ" nếu nó chưa tồn tại -> Không cần check null
                await _client
                    .Child("users")
                    .Child(_userId)
                    .Child("vocab")
                    .Child(cleanWord)
                    .Child("examples")
                    .PostAsync(exampleData);
            }
        }
    }
}