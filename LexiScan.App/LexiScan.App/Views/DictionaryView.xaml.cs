using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using LexiScan.App.ViewModels; // Nhớ using namespace ViewModel của bạn nếu cần ép kiểu

namespace LexiScan.App.Views
{
    public partial class DictionaryView : UserControl
    {
        public DictionaryView()
        {
            InitializeComponent();
        }

        // --- ĐOẠN CODE XỬ LÝ ĐIỀU HƯỚNG ---
        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Kiểm tra nếu danh sách gợi ý đang không có gì hoặc đang ẩn thì thôi
            if (SuggestionListBox.Items.Count == 0 || SuggestionListBox.Visibility != Visibility.Visible)
            {
                return;
            }

            // 1. XỬ LÝ PHÍM MŨI TÊN XUỐNG
            if (e.Key == Key.Down)
            {
                // Nếu chưa chọn gì -> Chọn cái đầu tiên
                if (SuggestionListBox.SelectedIndex < 0)
                {
                    SuggestionListBox.SelectedIndex = 0;
                }
                // Nếu chưa phải cái cuối -> Chọn cái tiếp theo
                else if (SuggestionListBox.SelectedIndex < SuggestionListBox.Items.Count - 1)
                {
                    SuggestionListBox.SelectedIndex++;
                }

                // Cuộn xuống để user thấy từ đang chọn
                SuggestionListBox.ScrollIntoView(SuggestionListBox.SelectedItem);
                e.Handled = true; // Ngăn con trỏ trong TextBox di chuyển lung tung
            }

            // 2. XỬ LÝ PHÍM MŨI TÊN LÊN
            else if (e.Key == Key.Up)
            {
                // Nếu đang ở vị trí > 0 -> Chọn cái bên trên
                if (SuggestionListBox.SelectedIndex > 0)
                {
                    SuggestionListBox.SelectedIndex--;
                    SuggestionListBox.ScrollIntoView(SuggestionListBox.SelectedItem);
                }
                // Nếu đang ở đầu -> Bỏ chọn (để quay lại gõ phím)
                else if (SuggestionListBox.SelectedIndex == 0)
                {
                    SuggestionListBox.SelectedIndex = -1;
                }
                e.Handled = true;
            }

            // 3. XỬ LÝ PHÍM ENTER
            else if (e.Key == Key.Enter)
            {
                // Nếu đang có từ được chọn ở dưới ListBox
                if (SuggestionListBox.SelectedItem != null)
                {
                    // Lấy từ đó ra
                    string selectedWord = SuggestionListBox.SelectedItem.ToString();

                    // Gán ngược lại vào ô Search (để ViewModel nhận được từ chính xác)
                    SearchTextBox.Text = selectedWord;

                    // Đưa con trỏ xuống cuối dòng
                    SearchTextBox.CaretIndex = SearchTextBox.Text.Length;

                    // Lưu ý: Sau khi chạy xong đoạn này, cái KeyBinding "Return" 
                    // trong XAML sẽ tự động kích hoạt SearchCommand.
                }
            }
        }
        // Thêm hàm này vào dưới hàm SearchTextBox_PreviewKeyDown
        private void SuggestionListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Kiểm tra xem có click đúng vào item không (tránh click vào vùng trắng)
            var item = ItemsControl.ContainerFromElement(SuggestionListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null && SuggestionListBox.SelectedItem != null)
            {
                // 1. Lấy từ được chọn điền vào ô tìm kiếm
                string selectedWord = SuggestionListBox.SelectedItem.ToString();
                SearchTextBox.Text = selectedWord;
                SearchTextBox.CaretIndex = SearchTextBox.Text.Length;

                // 2. Ẩn bảng gợi ý đi
                SuggestionListBox.Visibility = Visibility.Collapsed;

                // 3. Gọi lệnh Search (Vì ta đã xóa auto-search trong ViewModel nên giờ phải gọi thủ công)
                // Lưu ý: Bạn cần ép kiểu DataContext về ViewModel của bạn để gọi lệnh
                if (DataContext is LexiScan.App.ViewModels.DictionaryViewModel vm) // Thay DictionaryViewModel bằng tên ViewModel thực của bạn
                {
                    if (vm.SearchCommand.CanExecute(null))
                    {
                        vm.SearchCommand.Execute(null);
                    }
                }
            }
        }
    }
}