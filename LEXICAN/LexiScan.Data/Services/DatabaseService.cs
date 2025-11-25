// Project: LexiScan.Data
// File: Services/DatabaseService.cs
using System;
using System.Linq;
using LexiScan.Data.Models;
using Microsoft.EntityFrameworkCore;
public class DatabaseService
{
    private readonly LexiScanDbContext _context;

    public DatabaseService()
    {
        _context = new LexiScanDbContext();
    }

    /// <summary>
    /// Nhiệm vụ Tuần 2: Đảo ngược trạng thái Pin (Ghim) của một bản ghi. [cite: 56]
                /// </summary>
    public bool TogglePinStatus(int sentenceId)
    {
        var sentence = _context.Sentences.Find(sentenceId);

        if (sentence != null)
        {
            sentence.IsPinned = !sentence.IsPinned;
            _context.SaveChanges();
            return sentence.IsPinned;
        }
        return false;
    }

    /// <summary>
    /// Nhiệm vụ Tuần 2: Xóa tất cả lịch sử của một từ/câu nhất định. [cite: 56]
                /// </summary>
    public void DeleteHistory(string word)
    {
        var recordsToDelete = _context.Sentences
                                    .Where(s => s.SourceText == word)
                                    .ToList();

        if (recordsToDelete.Any())
        {
            _context.Sentences.RemoveRange(recordsToDelete);
            _context.SaveChanges();
        }
    }
}