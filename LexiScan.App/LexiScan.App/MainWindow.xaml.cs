using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LexiScan.App.ViewModels;
using ScreenTranslator; // <--- QUAN TRỌNG: Thêm dòng này để dùng TrayService

namespace LexiScan.App
{
    public partial class MainWindow : Window
    {
        // 1. Khai báo biến để giữ TrayService hoạt động
        private TrayService _trayService;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();

            // 2. KHỞI TẠO TRAY SERVICE
            // Truyền 'this' (cửa sổ hiện tại) vào để TrayService biết cửa sổ nào cần hiện lên
            _trayService = new TrayService(this);
            _trayService.Initialize();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            // 3. SỬA NÚT ĐÓNG THÀNH ẨN
            // Nếu dùng Shutdown() thì App tắt luôn, mất cả icon dưới khay.
            // Dùng Hide() để ẩn giao diện đi nhưng App vẫn chạy ngầm.
            this.Hide();
        }
    }
}