using LexiScan.App; // Giúp tìm thấy MainWindow
using LexiScan.App.Commands;
using LexiScan.Core.Services; // Namespace chứa AuthService
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LexiScan.App.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        // ==========================================================
        // PHẦN 1: VISIBILITY & DATA
        // ==========================================================
        private bool _isLoginVisible = true;
        public bool IsLoginVisible
        {
            get => _isLoginVisible;
            set { _isLoginVisible = value; OnPropertyChanged(); }
        }

        private bool _isSignUpVisible = false;
        public bool IsSignUpVisible
        {
            get => _isSignUpVisible;
            set { _isSignUpVisible = value; OnPropertyChanged(); }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _regEmail;
        public string RegEmail
        {
            get => _regEmail;
            set { _regEmail = value; OnPropertyChanged(); }
        }

        // ==========================================================
        // PHẦN 2: COMMANDS
        // ==========================================================
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

        // --- 1. Đăng nhập & Lưu Token & Mở MainWindow ---
        private async void ExecuteLogin(object? parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập Email và Mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // [MỚI] Gọi hàm lấy Token (Thay vì chỉ LoginAsync trả về bool)
            // Lưu ý: Đảm bảo bạn đã cập nhật AuthService có hàm này như hướng dẫn trước
            string token = await _authService.LoginAndGetTokenAsync(Username, password);

            // Kiểm tra nếu có Token (nghĩa là đăng nhập thành công)
            if (!string.IsNullOrEmpty(token))
            {
                // [MỚI] Lưu Token vào Settings của máy
                LexiScan.App.Properties.Settings.Default.UserToken = token;
                LexiScan.App.Properties.Settings.Default.Save(); // Bắt buộc phải Save

                // --- CHUYỂN MÀN HÌNH ---
                var mainWindow = new MainWindow();
                mainWindow.Show();
                CloseWindow();
            }
            else
            {
                MessageBox.Show("Đăng nhập thất bại. Vui lòng kiểm tra lại Email/Password.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- 2. Đăng ký (Giữ nguyên) ---
        private async void ExecuteRegister(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(RegEmail))
            {
                MessageBox.Show("Vui lòng điền Email.", "Thiếu thông tin");
                return;
            }

            var passBox = parameter as PasswordBox;
            var password = passBox?.Password;

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu."); return;
            }

            bool isSuccess = await _authService.RegisterAsync(RegEmail, password);

            if (isSuccess)
            {
                MessageBox.Show("Đăng ký thành công! Vui lòng đăng nhập.", "Chúc mừng");
                IsSignUpVisible = false;
                IsLoginVisible = true;
                RegEmail = "";
            }
            else
            {
                MessageBox.Show("Đăng ký thất bại. Email có thể đã tồn tại hoặc không hợp lệ.", "Lỗi");
            }
        }

        // --- 3. Quên mật khẩu (Giữ nguyên) ---
        private async void ExecuteForgotPassword(object? parameter)
        {
            if (string.IsNullOrEmpty(Username))
            {
                MessageBox.Show("Vui lòng nhập Email vào ô đăng nhập để lấy lại mật khẩu.", "Thông báo");
                return;
            }

            var result = MessageBox.Show($"Gửi email đặt lại mật khẩu tới: {Username}?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                bool isSent = await _authService.ResetPasswordAsync(Username);
                if (isSent)
                    MessageBox.Show("Đã gửi email! Vui lòng kiểm tra hộp thư.", "Thành công");
                else
                    MessageBox.Show("Gửi thất bại. Email không tồn tại.", "Lỗi");
            }
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}