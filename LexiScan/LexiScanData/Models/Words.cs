using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace LexiScanData.Models // <--- Kiểm tra kỹ dòng này
{
    // Phải có chữ "public" ở đầu thì Project khác mới thấy được
    public class Word
    {
        public int WordId { get; set; }
        public string Text { get; set; }
        public string Meaning { get; set; }
        public string Pronunciation { get; set; }
        public string WordType { get; set; }
        public DateTime Timestamp { get; set; } // Thêm cái này để lưu lịch sử
    }
}

