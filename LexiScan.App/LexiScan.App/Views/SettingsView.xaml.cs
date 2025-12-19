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
                e.Handled = true; // Chặn phím lan ra ngoài

                // 1. Lấy phím thực tế (Xử lý phím System như Alt, F10)
                var key = (e.Key == Key.System ? e.SystemKey : e.Key);

                // 2. Bỏ qua nếu người dùng CHỈ mới đè các phím chức năng mà chưa bấm phím chính
                if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                    key == Key.LeftAlt || key == Key.RightAlt ||
                    key == Key.LeftShift || key == Key.RightShift ||
                    key == Key.LWin || key == Key.RWin)
                {
                    return;
                }

                // 3. Xây dựng chuỗi hiển thị
                var modifiers = Keyboard.Modifiers;
                StringBuilder sb = new StringBuilder();

                // Cộng dồn các phím chức năng (Nếu có)
                if ((modifiers & ModifierKeys.Control) != 0) sb.Append("Ctrl + ");
                if ((modifiers & ModifierKeys.Alt) != 0) sb.Append("Alt + ");
                if ((modifiers & ModifierKeys.Shift) != 0) sb.Append("Shift + ");
                if ((modifiers & ModifierKeys.Windows) != 0) sb.Append("Win + ");

                // 4. Lấy tên đẹp của phím (Hàm tự viết ở dưới)
                string niceKeyName = GetSafeKeyName(key);

                sb.Append(niceKeyName);

                // 5. Gửi về ViewModel
                vm.UpdateHotkey(sb.ToString());
            }
        }

        // --- HÀM DỊCH TÊN PHÍM TỪ "NGÔN NGỮ MÁY" SANG "NGÔN NGỮ NGƯỜI" ---
        private string GetSafeKeyName(Key key)
        {
            // Xử lý các phím số (D0 -> D9 thành 0 -> 9)
            if (key >= Key.D0 && key <= Key.D9)
                return key.ToString().Replace("D", "");

            // Xử lý các phím NumPad (NumPad0 -> 0)
            if (key >= Key.NumPad0 && key <= Key.NumPad9)
                return key.ToString().Replace("NumPad", "");

            // Xử lý các ký tự đặc biệt (Mapping thủ công cho đẹp)
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

                // Mặc định: Trả về tên gốc (A, B, C, F1, F2...)
                default: return key.ToString();
            }
        }
    }
}