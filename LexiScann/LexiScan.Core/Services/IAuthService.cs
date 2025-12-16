using System;
using System.Threading.Tasks;
using Firebase.Auth;         
using Firebase.Auth.Providers; 

namespace LexiScan.Core.Services
{
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

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                await _authClient.SignInWithEmailAndPasswordAsync(email, password);

                return true; 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi đăng nhập: " + ex.Message);
                return false; 
            }
        }

        public async Task<bool> RegisterAsync(string email, string password, string displayName = "")
        {
            try
            {
                await _authClient.CreateUserWithEmailAndPasswordAsync(email, password, displayName);

                return true; 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi đăng ký: " + ex.Message);
                return false; 
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                // Hàm này bảo Firebase: "Ê, gửi email đổi pass cho ông này hộ tôi"
                await _authClient.ResetEmailPasswordAsync(email);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi gửi email reset: " + ex.Message);
                return false; // Thường lỗi do email không tồn tại hoặc sai định dạng
            }
        }

    }
}