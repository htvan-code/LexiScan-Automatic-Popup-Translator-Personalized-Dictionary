using Firebase.Database;
using Firebase.Database.Query;
using LexiScanData.Models;
using System.Threading.Tasks;

namespace LexiScanData.Services
{
    public class UserDataService
    {
        private const string DbUrl = "https://lexiscan-authentication-default-rtdb.asia-southeast1.firebasedatabase.app/";
        private readonly FirebaseClient _client;
        private readonly string _userId;

        public UserDataService(string userId)
        {
            _client = new FirebaseClient(DbUrl);
            _userId = userId;
        }

        // Lưu thông tin cá nhân
        public async Task SaveProfileAsync(UserProfile profile)
        {
            await _client
                .Child("users")
                .Child(_userId)
                .Child("profile")
                .PutAsync(profile); // Dùng PutAsync để ghi đè
        }

        // Lấy thông tin cá nhân
        public async Task<UserProfile> GetProfileAsync()
        {
            return await _client
                .Child("users")
                .Child(_userId)
                .Child("profile")
                .OnceSingleAsync<UserProfile>();
        }
    }
}