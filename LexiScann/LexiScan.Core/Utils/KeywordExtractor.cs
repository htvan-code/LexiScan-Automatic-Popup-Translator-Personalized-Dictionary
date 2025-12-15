using System.Collections.Generic;
using System.Linq;

namespace LexiScan.Core.Utils
{
    public static class KeywordExtractor
    {
        private static readonly HashSet<string> StopWords = new()
        {
            "a","an","the","is","are","was","were","of","in","on","at",
            "to","for","and","but","or","it","he","she","they","we"
        };

        public static List<string> Extract(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence)) return new();

            var clean = new string(sentence
                .Where(c => char.IsLetter(c) || char.IsWhiteSpace(c))
                .ToArray())
                .ToLower();

            return clean
                .Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2 && !StopWords.Contains(w))
                .Distinct()
                .ToList();
        }
    }
}
