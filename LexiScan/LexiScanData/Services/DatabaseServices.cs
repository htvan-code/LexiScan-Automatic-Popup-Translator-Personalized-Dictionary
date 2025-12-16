using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexiScanData.Models;


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
        public void TestConnection()
        {
            using var context = new LexiScanDbContext();
            int count = context.Sentences.Count();
        }
        public void SavePinnedWords(int sentenceId, List<string> words)
        {
            using var context = new LexiScanDbContext();

            // 1. Lấy câu
            var sentence = context.Sentences.Find(sentenceId);
            if (sentence == null) return;

            sentence.IsPinned = true;

            foreach (var wordText in words)
            {
                // 2. Kiểm tra từ đã tồn tại chưa
                var word = context.Words.FirstOrDefault(w => w.Text == wordText);

                if (word == null)
                {
                    word = new Words { Text = wordText };
                    context.Words.Add(word);
                    context.SaveChanges();
                }

                // 3. Gắn từ với câu
                bool exists = context.SentenceWords.Any(sw =>
                    sw.SentenceId == sentenceId && sw.WordId == word.WordId);

                if (!exists)
                {
                    context.SentenceWords.Add(new SentenceWord
                    {
                        SentenceId = sentenceId,
                        WordId = word.WordId
                    });
                }
            }

            context.SaveChanges();
        }

    }
}
