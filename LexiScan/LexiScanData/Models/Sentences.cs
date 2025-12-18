using System;

namespace LexiScanData.Models
{
    public class Sentences
    {
        // [QUAN TRỌNG] Đổi từ int sang string để khớp với Firebase Key
        public string SentenceId { get; set; }

        public string SourceText { get; set; }

        public string TranslatedText { get; set; }

        public DateTime CreatedDate { get; set; }

        // Dòng này bạn đã có, nhưng hãy đảm bảo đã bấm Ctrl+S để lưu file
        public bool IsPinned { get; set; } = false;
    }
}