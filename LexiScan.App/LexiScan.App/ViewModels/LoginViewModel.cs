using LexiScan.App.Commands;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LexiScan.App.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        // ==========================================================
        // PHẦN 1: CÁC BIẾN CHO GIAO DIỆN (VISIBILITY & DATA)
        // ==========================================================

        // 1. Biến điều khiển ẩn hiện: Login vs SignUp
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

        // 2. Dữ liệu cho phần Đăng nhập
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        // 3. Dữ liệu cho phần Đăng ký (Binding từ giao diện SignUp)
        private string _regDisplayName; // Tên người dùng
        public string RegDisplayName
        {
            get => _regDisplayName;
            set { _regDisplayName = value; OnPropertyChanged(); }
        }

        private string _regEmail; // Email đăng ký
        public string RegEmail
        {
            get => _regEmail;
            set { _regEmail = value; OnPropertyChanged(); }
        }

        // ==========================================================
        // PHẦN 2: COMMANDS (CÁC LỆNH)
        // ==========================================================

        public ICommand LoginCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand SwitchToSignUpCommand { get; } // Chuyển sang Đăng ký
        public ICommand SwitchToLoginCommand { get; }  // Chuyển sang Đăng nhập
        public ICommand RegisterCommand { get; }       // Xử lý Đăng ký

        // ==========================================================
        // PHẦN 3: CONSTRUCTOR & LOGIC
        // ==========================================================

        public LoginViewModel()
        {
            // Giá trị mặc định để test
            Username = "admin";

            // Khởi tạo Commands
            LoginCommand = new RelayCommand(ExecuteLogin);
            CloseCommand = new RelayCommand(ExecuteClose);

            // Logic chuyển đổi giao diện
            SwitchToSignUpCommand = new RelayCommand(param =>
            {
                IsLoginVisible = false;
                IsSignUpVisible = true;
            });

            SwitchToLoginCommand = new RelayCommand(param =>
            {
                IsLoginVisible = true;
                IsSignUpVisible = false;
            });

            // Logic đăng ký
            RegisterCommand = new RelayCommand(ExecuteRegister);
        }

        // --- Xử lý Đăng nhập ---
        private void ExecuteLogin(object? parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password;

            // Logic kiểm tra đơn giản (Demo)
            if (Username == "admin" && password == "123")
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();

                // Đóng cửa sổ Login
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this) { window.Close(); break; }
                }
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu! (Thử: admin/123)", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Xử lý Đăng ký ---
        private void ExecuteRegister(object? parameter)
        {
            // Lưu ý: parameter ở đây nên là PasswordBox của phần đăng ký nếu muốn lấy mật khẩu
            // Ở đây demo kiểm tra thông tin cơ bản
            if (string.IsNullOrWhiteSpace(RegDisplayName) || string.IsNullOrWhiteSpace(RegEmail))
            {
                MessageBox.Show("Vui lòng điền đầy đủ Tên và Email!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Viết logic lưu vào SQL Server tại đây

            MessageBox.Show($"Đăng ký thành công tài khoản: {RegEmail}\nVui lòng đăng nhập lại.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            // Đăng ký xong thì chuyển về màn hình Login
            IsSignUpVisible = false;
            IsLoginVisible = true;

            // Reset các trường
            RegDisplayName = "";
            RegEmail = "";
        }

        // --- Xử lý Đóng ứng dụng ---
        private void ExecuteClose(object? _)
        {
            Application.Current.Shutdown();
        }
    }
}