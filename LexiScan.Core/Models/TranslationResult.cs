using System;
using System.Collections.Generic;
using LexiScan.Core.Enums;

namespace LexiScan.Core.Models
{
    public class TranslationResult
    {
        // === Dữ liệu Đầu vào/Đầu ra ===
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }

        // === Trạng thái & Phân loại ===
        public InputType InputType { get; set; }
        public ServiceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsError => Status != ServiceStatus.Success;
        public string ErrorMessage { get; set; }

        public bool IsFromClipboard { get; set; }

        // === Dictionary Data ===
        public string Phonetic { get; set; }
        public List<Meaning> Meanings { get; set; } = new List<Meaning>();
    }
}