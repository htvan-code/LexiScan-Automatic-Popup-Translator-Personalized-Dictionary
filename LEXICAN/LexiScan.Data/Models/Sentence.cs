// Project: LexiScan.Data
// File: Models/Sentence.cs
using System;
using System.ComponentModel.DataAnnotations;

public class Sentence
{
    public int SentenceId { get; set; }
    public string SourceText { get; set; }
    public string TranslatedText { get; set; }
    public DateTime CreatedDate { get; set; }

    // Cột mới để hỗ trợ "pinned" [cite: 31, 32]
    public bool IsPinned { get; set; } = false;
}
