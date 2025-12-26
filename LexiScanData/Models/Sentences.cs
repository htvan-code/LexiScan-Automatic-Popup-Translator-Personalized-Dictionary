public class Sentences
{
    public string SentenceId { get; set; } // Phải là string
    public string SourceText { get; set; }
    public string TranslatedText { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool IsPinned { get; set; } = false;
}