using System;
using System.Windows;

namespace LexiScan.App.Services
{
    public static class ThemeService
    {
        private static readonly Uri DarkTheme =
            new Uri("pack://application:,,,/LexiScan.App;component/Themes/DarkTheme.xaml");

        private static readonly Uri LightTheme =
            new Uri("pack://application:,,,/LexiScan.App;component/Themes/LightTheme.xaml");

        public static bool IsDarkMode { get; private set; } = true;

        public static void ToggleTheme()
        {
            var appResources = Application.Current.Resources.MergedDictionaries;

            // Xóa theme cũ
            for (int i = appResources.Count - 1; i >= 0; i--)
            {
                var dict = appResources[i];
                if (dict.Source == DarkTheme || dict.Source == LightTheme)
                {
                    appResources.RemoveAt(i);
                }
            }

            // Thêm theme mới
            appResources.Add(new ResourceDictionary
            {
                Source = IsDarkMode ? LightTheme : DarkTheme
            });

            IsDarkMode = !IsDarkMode;
        }
    }
}
