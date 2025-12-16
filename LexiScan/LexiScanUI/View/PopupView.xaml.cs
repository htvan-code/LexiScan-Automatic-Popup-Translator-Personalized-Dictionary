using System.Windows;
using System.Windows.Controls;
using LexiScanUI.ViewModels;

namespace LexiScanUI.View
{
    public partial class PopupView : UserControl
    {
        public PopupView()
        {
            InitializeComponent();
        }

        // Helper lấy ViewModel
        private PopupViewModel? ViewModel => DataContext as PopupViewModel;

        // ========================
        // PIN
        // ========================
        private void Pin_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.PinCommand.CanExecute(null) == true)
            {
                ViewModel.PinCommand.Execute(null);
            }
        }

        // ========================
        // SETTINGS
        // ========================
        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.SettingsCommand.CanExecute(null) == true)
            {
                ViewModel.SettingsCommand.Execute(null);
            }
        }

        // ========================
        // CLOSE POPUP
        // ========================
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
