using System;
using System.Windows;
using System.Windows.Controls;
using LexiScan.App.ViewModels;
using LexiScan.App.Services; // Thêm namespace Service

namespace LexiScan.App
{
    public partial class MainWindow : Window
    {
        // Khóa để xác định Resource Dictionary nào đã được tải
        private const string LightThemeUri = "Themes/LightTheme.xaml";
        private const string DarkThemeUri = "Themes/DarkTheme.xaml";

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();

            // Đăng ký lắng nghe sự kiện thay đổi theme
            ThemeService.Instance.ThemeChanged += OnThemeChanged;

            // Tải theme mặc định khi khởi động (chúng ta sẽ mặc định là Light Theme)
            LoadTheme(LightThemeUri);
        }

        private void OnThemeChanged(string themeName)
        {
            string themeUri = themeName == "Dark" ? DarkThemeUri : LightThemeUri;
            LoadTheme(themeUri);
        }

        private void LoadTheme(string themeUri)
        {
            // Bước 1: Loại bỏ Resource Dictionary theme cũ (nếu có)
            var currentDictionaries = Application.Current.Resources.MergedDictionaries;

            // Tìm và loại bỏ bất kỳ theme nào trước đó đã được load
            for (int i = currentDictionaries.Count - 1; i >= 0; i--)
            {
                var dictionary = currentDictionaries[i];
                if (dictionary.Source != null && (dictionary.Source.OriginalString.Contains("Theme.xaml")))
                {
                    currentDictionaries.RemoveAt(i);
                }
            }

            // Bước 2: Thêm Resource Dictionary theme mới
            ResourceDictionary newTheme = new ResourceDictionary
            {
                Source = new Uri(themeUri, UriKind.Relative)
            };
            currentDictionaries.Add(newTheme);
        }

        // Đảm bảo hủy đăng ký sự kiện khi cửa sổ đóng
        protected override void OnClosed(EventArgs e)
        {
            ThemeService.Instance.ThemeChanged -= OnThemeChanged;
            base.OnClosed(e);
        }
    }
}