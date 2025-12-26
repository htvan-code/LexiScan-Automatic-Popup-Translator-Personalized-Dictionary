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

        // xử lí điều hướng
        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (SuggestionListBox.Items.Count == 0 || SuggestionListBox.Visibility != Visibility.Visible)
            {
                return;
            }

            // 1. XỬ LÝ PHÍM MŨI TÊN XUỐNG
            if (e.Key == Key.Down)
            {
                if (SuggestionListBox.SelectedIndex < 0)
                {
                    SuggestionListBox.SelectedIndex = 0;
                }
                else if (SuggestionListBox.SelectedIndex < SuggestionListBox.Items.Count - 1)
                {
                    SuggestionListBox.SelectedIndex++;
                }
                SuggestionListBox.ScrollIntoView(SuggestionListBox.SelectedItem);
                e.Handled = true; // Ngăn con trỏ trong TextBox di chuyển lung tung
            }

            // 2. XỬ LÝ PHÍM MŨI TÊN LÊN
            else if (e.Key == Key.Up)
            {
                
                if (SuggestionListBox.SelectedIndex > 0)
                {
                    SuggestionListBox.SelectedIndex--;
                    SuggestionListBox.ScrollIntoView(SuggestionListBox.SelectedItem);
                }
 
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
                    string selectedWord = SuggestionListBox.SelectedItem.ToString();
                    SearchTextBox.Text = selectedWord;
                    SearchTextBox.CaretIndex = SearchTextBox.Text.Length;
                }
            }
        }

        private void SuggestionListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            var item = ItemsControl.ContainerFromElement(SuggestionListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null && SuggestionListBox.SelectedItem != null)
            {
        
                string selectedWord = SuggestionListBox.SelectedItem.ToString();
                SearchTextBox.Text = selectedWord;
                SearchTextBox.CaretIndex = SearchTextBox.Text.Length;

     
                SuggestionListBox.Visibility = Visibility.Collapsed;

           
                if (DataContext is LexiScan.App.ViewModels.DictionaryViewModel vm) 
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