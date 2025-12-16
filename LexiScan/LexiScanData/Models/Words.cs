using System;

namespace LexiScanData.Models // <--- Quan trọng: Namespace phải đúng
{
    public class Word // <--- Phải là "public" và tên là "Word"
    {
        public int WordId { get; set; }
        public string Text { get; set; }
        public string Meaning { get; set; }
        public string Pronunciation { get; set; }
        public string WordType { get; set; }
        public DateTime Timestamp { get; set; }
    }
}