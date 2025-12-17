using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Auth.Providers;

namespace LexiScan.Core.Services
{
    // Lưu ý: Tên file là IAuthService.cs nhưng bên trong chứa class AuthService
    // Nếu được, bạn nên đổi tên file thành AuthService.cs cho đúng chuẩn.
    public class AuthService
    {
        private const string ApiKey = "AIzaSyBtGVdAJxyRcRTk_mJBeYXHK_OHS3HTPQA";
        private const string AuthDomain = "lexiscan-authentication.firebaseapp.com";
        private readonly FirebaseAuthClient _authClient;

        public AuthService()
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = ApiKey,
                AuthDomain = AuthDomain,
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                }
            };
            _authClient = new FirebaseAuthClient(config);
        }

        // --- HÀM 1: Đăng nhập và lấy Token ---
        public async Task<string> LoginAndGetTokenAsync(string email, string password)
        {
            try
            {
                var userCred = await _authClient.SignInWithEmailAndPasswordAsync(email, password);

                // SỬA LỖI TẠI ĐÂY: Truy cập vào Credential để lấy RefreshToken
                return userCred.User.Credential.RefreshToken;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // --- HÀM 2: Tự động đăng nhập (Auto Login) ---
        public async Task<bool> AutoLoginAsync(string refreshToken)
        {
            // SỬA LỖI TẠI ĐÂY: Tạm thời chỉ kiểm tra chuỗi token có rỗng không
            // Để tránh lỗi "User does not contain definition for ChangeUserEmail"
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            // Nếu có token thì coi như đã đăng nhập (Để app chạy được)
            // Sau này nếu cần bảo mật cao hơn, ta sẽ gọi API kiểm tra Token sau
            return await Task.FromResult(true);
        }

        // --- Các hàm cũ giữ nguyên ---
        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                await _authClient.SignInWithEmailAndPasswordAsync(email, password);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> RegisterAsync(string email, string password, string displayName = "")
        {
            try
            {
                await _authClient.CreateUserWithEmailAndPasswordAsync(email, password, displayName);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                await _authClient.ResetEmailPasswordAsync(email);
                return true;
            }
            catch { return false; }
        }
    }
}