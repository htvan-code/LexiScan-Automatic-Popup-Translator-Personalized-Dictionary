using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexiScanData.Models
{
    internal class Sentences
    {
        public int SentenceId { get; set; }
        public string SourceText { get; set; }
        public string TranslatedText { get; set; }
        public DateTime CreatedDate { get; set; }

        // Cột mới để hỗ trợ "pinned" [cite: 31, 32]
        public bool IsPinned { get; set; } = false;
    }
}
