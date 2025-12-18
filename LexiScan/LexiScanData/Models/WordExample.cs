using System;

namespace LexiScanData.Models
{
    public class WordExample
    {
        public string OriginalSentence { get; set; }
        public string TranslatedMeaning { get; set; }
        public DateTime SavedDate { get; set; } = DateTime.Now;
    }
}