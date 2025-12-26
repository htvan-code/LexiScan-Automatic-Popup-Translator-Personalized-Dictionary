using LexiScan.App;
using LexiScan.App.Commands;
using LexiScan.Core.Services;
using LexiScan.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LexiScan.App.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        private bool _isLoginVisible = true;
        public bool IsLoginVisible { get => _isLoginVisible; set { _isLoginVisible = value; OnPropertyChanged(); } }

        private bool _isSignUpVisible = false;
        public bool IsSignUpVisible { get => _isSignUpVisible; set { _isSignUpVisible = value; OnPropertyChanged(); } }

        private string _username;
        public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }

        private string _regEmail;
        public string RegEmail { get => _regEmail; set { _regEmail = value; OnPropertyChanged(); } }

        public ICommand LoginCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand SwitchToSignUpCommand { get; }
        public ICommand SwitchToLoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        public LoginViewModel()
        {
            _authService = new AuthService();

            SwitchToSignUpCommand = new RelayCommand(p => { IsLoginVisible = false; IsSignUpVisible = true; });
            SwitchToLoginCommand = new RelayCommand(p => { IsLoginVisible = true; IsSignUpVisible = false; });
            CloseCommand = new RelayCommand(p => Application.Current.Shutdown());

            LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
            ForgotPasswordCommand = new RelayCommand(ExecuteForgotPassword);
        }


        private async void ExecuteLogin(object? parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập Email và Mật khẩu!", "Thông báo");
                return;
            }

            try
            {
                var user = await _authService.LoginAndGetUserAsync(Username, password);

                if (user != null)
                {
                    string refreshToken = user.Credential.RefreshToken;

                    var settings = LexiScan.App.Properties.Settings.Default;
                    settings.UserId = user.Uid;
                    settings.RefreshToken = refreshToken;

                    settings.Save();

                    var uid = user.Uid;
                    var token = await user.GetIdTokenAsync();

                    LexiScan.App.Properties.Settings.Default.UserId = uid;
                    LexiScan.App.Properties.Settings.Default.Save();

                    LexiScan.Core.SessionManager.CurrentUserId = uid;
                    LexiScan.Core.SessionManager.CurrentAuthToken = token;

                    CloseWindow(true);
                }
                else
                {
                    MessageBox.Show("Đăng nhập thất bại. Kiểm tra lại thông tin.", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message, "Lỗi");
            }
        }
        private async void ExecuteRegister(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(RegEmail)) return;
            var passBox = parameter as PasswordBox;
            var password = passBox?.Password;
            if (await _authService.RegisterAsync(RegEmail, password))
            {
                MessageBox.Show("Đăng ký thành công!");
                IsSignUpVisible = false; IsLoginVisible = true;
            }
            else MessageBox.Show("Đăng ký thất bại.");
        }

        private async void ExecuteForgotPassword(object? parameter)
        {
            if (!string.IsNullOrEmpty(Username)) await _authService.ResetPasswordAsync(Username);
        }

        private void CloseWindow(bool isSuccess)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    if (isSuccess) window.DialogResult = true;
                    window.Close();
                    break;
                }
            }
        }
    }
}