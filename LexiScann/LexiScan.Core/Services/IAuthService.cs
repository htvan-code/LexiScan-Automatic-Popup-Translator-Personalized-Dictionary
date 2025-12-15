using System.Threading.Tasks;
using LexiScan.Core.Models;

namespace LexiScan.Core.Services
{
    public interface IAuthService
    {
        Task<User> LoginAsync(string email, string password);
        Task<User> RegisterAsync(string email, string password);
        void Logout();
        User CurrentUser { get; }
    }
}