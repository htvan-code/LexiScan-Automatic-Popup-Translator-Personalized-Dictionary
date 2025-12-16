using LexiScanData.Models;
using System.Collections.Generic;
using System.Linq;

namespace LexiScanData.Services
{
    public class DatabaseServices
    {
        private readonly LexiScanDbContext _context;

        public DatabaseServices()
        {
            _context = new LexiScanDbContext();
        }

        // =====================================================
        // =============== PERSON 4 – PIN WORD =================
        // =====================================================

        public bool TogglePinStatus(int sentenceId)
        {
            var sentence = _context.Sentences.Find(sentenceId);
            if (sentence == null) return false;

            sentence.IsPinned = !sentence.IsPinned;
            _context.SaveChanges();
            return sentence.IsPinned;
        }

        public void SavePinnedWords(int sentenceId, List<string> words)
        {
            var sentence = _context.Sentences.Find(sentenceId);
            if (sentence == null) return;

            sentence.IsPinned = true;

            foreach (var wordText in words)
            {
                // 1. Lấy hoặc tạo Word
                var word = _context.Words.FirstOrDefault(w => w.Text == wordText);
                if (word == null)
                {
                    word = new Word
                    {
                        Text = wordText,
                        Timestamp = System.DateTime.Now
                    };
                    _context.Words.Add(word);
                    _context.SaveChanges();
                }

                // 2. Gắn Word với Sentence
                bool exists = _context.SentenceWords.Any(sw =>
                    sw.SentenceId == sentenceId && sw.WordId == word.WordId);

                if (!exists)
                {
                    _context.SentenceWords.Add(new SentenceWord
                    {
                        SentenceId = sentenceId,
                        WordId = word.WordId
                    });
                }
            }

            _context.SaveChanges();
        }

        // =====================================================
        // =============== PERSON 3 – HISTORY ==================
        // =====================================================

        public List<Word> GetAllWords()
        {
            return _context.Words
                           .OrderByDescending(w => w.Timestamp)
                           .ToList();
        }

        public void DeleteWord(int wordId)
        {
            var word = _context.Words.FirstOrDefault(w => w.WordId == wordId);
            if (word == null) return;

            // Xóa quan hệ trước
            var links = _context.SentenceWords
                                .Where(sw => sw.WordId == wordId);
            _context.SentenceWords.RemoveRange(links);

            _context.Words.Remove(word);
            _context.SaveChanges();
        }

        public void ClearAllWords()
        {
            _context.SentenceWords.RemoveRange(_context.SentenceWords);
            _context.Words.RemoveRange(_context.Words);
            _context.SaveChanges();
        }

        // =====================================================
        // ================== DEBUG / TEST =====================
        // =====================================================

        public void TestConnection()
        {
            int count = _context.Sentences.Count();
        }
    }
}
