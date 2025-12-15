using System.Threading.Tasks;

namespace LexiScan.Core.Services
{
    public interface ISyncService
    {
        // Đồng bộ dữ liệu cục bộ lên Cloud
        Task<bool> PushDataAsync();

        // Kéo dữ liệu từ Cloud về cục bộ
        Task<bool> PullDataAsync();
    }
}