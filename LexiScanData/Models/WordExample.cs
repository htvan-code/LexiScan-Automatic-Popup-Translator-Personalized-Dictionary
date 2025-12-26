using System;

namespace LexiScanData.Models
{
    public class WordExample
    {
        public string SourceText { get; set; }
        public string Id { get; set; }

        public string Meaning { get; set; }

        public DateTime SavedDate { get; set; } = DateTime.Now;
    }
}