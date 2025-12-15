using System.Collections.Generic;
using System.Threading.Tasks;
using LexiScan.Core.Models;

namespace LexiScan.Core.Services
{
    // P4 phải implement để AppCoordinator (P2) sử dụng
    public interface IDatabaseService
    {
        // P2 gọi hàm này sau khi dịch xong
        Task SaveTranslationResult(TranslationResult result, List<string> keywords);

        // Các hàm P3/P4 sẽ dùng để quản lý UI/History
        Task<List<TranslationResult>> GetHistoryAsync();
        Task DeleteHistoryAsync(int id);
        Task TogglePinStatusAsync(int id);
    }
}