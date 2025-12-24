using System;

namespace LexiScanData.Models
{
    public class WordExample
    {
        // Nội dung tiếng Anh (Từ hoặc Câu)
        public string SourceText { get; set; }

        // Nghĩa tiếng Việt
        public string Meaning { get; set; }

        // Ngày lưu
        public DateTime SavedDate { get; set; } = DateTime.Now;
    }
}