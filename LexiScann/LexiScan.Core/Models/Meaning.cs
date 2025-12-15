using System.Collections.Generic;

namespace LexiScan.Core.Models
{
    public class Meaning
    {
        public string PartOfSpeech { get; set; }

        public string Definition { get; set; }

        public List<string> Examples { get; set; } = new List<string>();
    }
}