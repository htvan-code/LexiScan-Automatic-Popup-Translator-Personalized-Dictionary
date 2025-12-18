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

        public async Task<Firebase.Auth.User> LoginAndGetUserAsync(string email, string password)
        {
            try
            {
                var userCred = await _authClient.SignInWithEmailAndPasswordAsync(email, password);

                // Trả về nguyên đối tượng User (chứa cả Token lẫn LocalId)
                return userCred.User;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> AutoLoginAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            return await Task.FromResult(true);
        }

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