using LexiScan.App.ViewModels;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace LexiScan.App.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            this.PreviewKeyDown += SettingsView_PreviewKeyDown;
        }

        private void SettingsView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is SettingsViewModel vm && vm.IsChangingHotkey)
            {
                e.Handled = true;
                var key = (e.Key == Key.System ? e.SystemKey : e.Key);

                if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                    key == Key.LeftAlt || key == Key.RightAlt ||
                    key == Key.LeftShift || key == Key.RightShift ||
                    key == Key.LWin || key == Key.RWin)
                {
                    return;
                }

                var modifiers = Keyboard.Modifiers;
                StringBuilder sb = new StringBuilder();

                if ((modifiers & ModifierKeys.Control) != 0) sb.Append("Ctrl + ");
                if ((modifiers & ModifierKeys.Alt) != 0) sb.Append("Alt + ");
                if ((modifiers & ModifierKeys.Shift) != 0) sb.Append("Shift + ");
                if ((modifiers & ModifierKeys.Windows) != 0) sb.Append("Win + ");

                sb.Append(GetSafeKeyName(key));
                vm.UpdateHotkey(sb.ToString());
            }
        }

        private string GetSafeKeyName(Key key)
        {
            if (key >= Key.D0 && key <= Key.D9) return key.ToString().Replace("D", "");
            if (key >= Key.NumPad0 && key <= Key.NumPad9) return key.ToString().Replace("NumPad", "");

            return key switch
            {
                Key.Oem3 => "~",
                Key.OemMinus => "-",
                Key.OemPlus => "+",
                Key.OemOpenBrackets => "[",
                Key.OemCloseBrackets => "]",
                Key.Oem5 => "\\",
                Key.Oem1 => ";",
                Key.OemQuotes => "'",
                Key.OemComma => ",",
                Key.OemPeriod => ".",
                Key.OemQuestion => "/",
                Key.Back => "Backspace",
                Key.Return => "Enter",
                Key.Escape => "Esc",
                Key.Delete => "Del",
                Key.Space => "Space",
                _ => key.ToString()
            };
        }
    }
}