namespace LexiScan.Services.Keywords;

public static class KeywordExtractor
{
    private static readonly HashSet<string> StopWords = new()
    {
        "the","is","at","which","on","and","a","an","to","of","for"
    };

    public static List<string> GetKeywords(string sentence)
    {
        return sentence
            .ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !StopWords.Contains(w))
            .Distinct()
            .ToList();
    }
}