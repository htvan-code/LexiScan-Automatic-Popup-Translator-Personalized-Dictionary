// LexiScan.App/MainWindow.xaml.cs

using System;
using System.Windows;
using System.Windows.Controls;
using LexiScan.App.ViewModels;
using System.Windows.Input;
// Xóa: using LexiScan.App.Services; 

namespace LexiScan.App
{
    public partial class MainWindow : Window
    {
        // Xóa các khai báo URI không cần thiết
        // private const string LightThemeUri = "Themes/LightTheme.xaml";
        // private const string DarkThemeUri = "Themes/DarkTheme.xaml";

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();

            // Xóa logic ThemeService và LoadTheme
            // ThemeService.Instance.ThemeChanged += OnThemeChanged;
            // LoadTheme(DarkThemeUri); 
        }

        // Xóa các phương thức liên quan đến Theme
        /*
        private void OnThemeChanged(string themeName) { ... }
        private void LoadTheme(string themeUri) { ... }
        protected override void OnClosed(EventArgs e) { ... } 
        */


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}