using LexiScan.App.Commands;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LexiScan.App.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand CloseCommand { get; }

        public LoginViewModel()
        {
            Username = "admin"; // Giá trị mặc định để test
            LoginCommand = new RelayCommand(ExecuteLogin);
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        private void ExecuteLogin(object? parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password;

            // Logic kiểm tra đơn giản
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
                MessageBox.Show("Sai tài khoản hoặc mật khẩu! (Thử: admin/123)");
            }
        }

        private void ExecuteClose(object? _)
        {
            Application.Current.Shutdown();
        }
    }
}