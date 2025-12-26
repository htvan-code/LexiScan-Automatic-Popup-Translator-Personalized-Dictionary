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
                string niceKeyName = GetSafeKeyName(key);

                sb.Append(niceKeyName);
                vm.UpdateHotkey(sb.ToString());
            }
        }

        private string GetSafeKeyName(Key key)
        {

            if (key >= Key.D0 && key <= Key.D9)
                return key.ToString().Replace("D", "");

            if (key >= Key.NumPad0 && key <= Key.NumPad9)
                return key.ToString().Replace("NumPad", "");

            switch (key)
            {
                case Key.Oem3: return "~";       // Phím dấu ngã / huyền
                case Key.OemMinus: return "-";
                case Key.OemPlus: return "+";
                case Key.OemOpenBrackets: return "[";
                case Key.OemCloseBrackets: return "]";
                case Key.Oem5: return "\\";      // Dấu gạch chéo ngược
                case Key.Oem1: return ";";
                case Key.OemQuotes: return "'";
                case Key.OemComma: return ",";
                case Key.OemPeriod: return ".";
                case Key.OemQuestion: return "/";

                case Key.Back: return "Backspace";
                case Key.Return: return "Enter";
                case Key.Escape: return "Esc";
                case Key.Delete: return "Del";
                default: return key.ToString();
            }
        }
    }
}