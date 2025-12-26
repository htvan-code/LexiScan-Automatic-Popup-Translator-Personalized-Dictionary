using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Auth.Providers;
using System.Net.Http; 
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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
                return userCred.User;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> AutoLoginAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) return null;

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });

                    var response = await client.PostAsync($"https://securetoken.googleapis.com/v1/token?key={ApiKey}", content);

                    if (!response.IsSuccessStatusCode) return null;

                    var json = await response.Content.ReadAsStringAsync();

                    if (json.Contains("id_token"))
                    {
                        var split1 = json.Split(new[] { "\"id_token\": \"" }, StringSplitOptions.None)[1];
                        var newToken = split1.Split('"')[0];
                        return newToken;
                    }
                }
            }
            catch { }

            return null; 
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