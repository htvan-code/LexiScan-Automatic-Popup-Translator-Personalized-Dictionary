using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexiScanData.Services
{
    public class DatabaseServices
    {
        private readonly LexiScanDbContext _context;

        public DatabaseServices()
        {
            _context = new LexiScanDbContext();
        }
        // LexiScanData/Services/DatabaseServices.cs

        public void SavePinnedWords(int sentenceId, List<string> selectedWords)
        {
            // Tìm câu hiện tại
            var sentence = _context.Sentences.Find(sentenceId);

            if (sentence != null)
            {
                // LOGIC LƯU: Tùy thuộc vào database của bạn thiết kế thế nào.
                // Ví dụ 1: Nếu bạn lưu các từ đã chọn thành một chuỗi phân cách bởi dấu phẩy
                string wordsToSave = string.Join(", ", selectedWords);

                // Giả sử model Sentences có trường Note hoặc PinnedWords để lưu
                // sentence.PinnedWords = wordsToSave; 

                // Ví dụ 2: Nếu chỉ đơn giản là đánh dấu (như code mẫu ViewModel đang set IsPinned = true)
                sentence.IsPinned = true;

                // Lưu thay đổi vào DB
                _context.SaveChanges();
            }
        }
        public bool TogglePinStatus(int sentenceId)
        {
            var sentence = _context.Sentences.Find(sentenceId);

            if (sentence != null)
            {
                sentence.IsPinned = !sentence.IsPinned;
                _context.SaveChanges();
                return sentence.IsPinned;
            }
            return false;
        }
        public void DeleteHistory(string word)
        {
            var recordsToDelete = _context.Sentences
                                        .Where(s => s.SourceText == word)
                                        .ToList();

            if (recordsToDelete.Any())
            {
                _context.Sentences.RemoveRange(recordsToDelete);
                _context.SaveChanges();
            }
        }
    }
}
