using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexiScanData.Services
{
    internal class DatabaseServices
    {
        private readonly LexiScanDbContext _context;

        public DatabaseServices()
        {
            _context = new LexiScanDbContext();
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
