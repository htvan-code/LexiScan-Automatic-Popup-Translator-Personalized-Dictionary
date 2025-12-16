using System.Collections.Generic;

namespace LexiScan.Core.Models
{
    public class Meaning
    {
        public string PartOfSpeech { get; set; } 

        public List<string> Definitions { get; set; } = new List<string>();

        public List<string> Examples { get; set; }
    }
}