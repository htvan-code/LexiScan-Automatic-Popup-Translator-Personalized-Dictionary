using System;
using System.Windows;

namespace LexiScan.App
{
    public partial class App : Application
    {
        public static void ChangeTheme(bool isDark)
        {
            var mergedDicts = Current.Resources.MergedDictionaries;

            // Xóa theme cũ
            if (mergedDicts.Count > 0)
            {
                // Giả định theme tùy chỉnh của bạn luôn nằm ở cuối cùng
                mergedDicts.RemoveAt(mergedDicts.Count - 1);
            }

            // Thêm theme mới với đường dẫn folder "Themes"
            var newTheme = new ResourceDictionary();
            if (isDark)
            {
                // SỬA ĐƯỜNG DẪN TẠI ĐÂY
                newTheme.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
            }
            else
            {
                // SỬA ĐƯỜNG DẪN TẠI ĐÂY
                newTheme.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
            }

            mergedDicts.Add(newTheme);
        }
    }
}