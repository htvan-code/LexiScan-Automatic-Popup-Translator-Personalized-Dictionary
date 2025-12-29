using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using LexiScan.App.ViewModels;
using System.Windows.Data;

namespace LexiScan.App.Views
{
    public partial class DictionaryView : UserControl
    {
        public DictionaryView()
        {
            InitializeComponent();
        }

        // Xử lý Click chuột
        private void SuggestionListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(SuggestionListBox, e.OriginalSource as DependencyObject) as ListBoxItem;

            if (item != null)
            {
                string selectedWord = item.Content.ToString();
                PerformSearch(selectedWord);
                e.Handled = true; 
            }
        }

        // Xử lý Bàn phím
        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
 
            if (SuggestionListBox.Items.Count == 0 || SuggestionListBox.Visibility != Visibility.Visible)
            {
                return;
            }

            if (e.Key == Key.Down)
            {
                if (SuggestionListBox.SelectedIndex < 0) SuggestionListBox.SelectedIndex = 0;
                else if (SuggestionListBox.SelectedIndex < SuggestionListBox.Items.Count - 1) SuggestionListBox.SelectedIndex++;

                SuggestionListBox.ScrollIntoView(SuggestionListBox.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (SuggestionListBox.SelectedIndex > 0) SuggestionListBox.SelectedIndex--;
                else if (SuggestionListBox.SelectedIndex == 0) SuggestionListBox.SelectedIndex = -1;

                SuggestionListBox.ScrollIntoView(SuggestionListBox.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (SuggestionListBox.SelectedItem != null)
                {
                    string selectedWord = SuggestionListBox.SelectedItem.ToString();
                    PerformSearch(selectedWord);
                    e.Handled = true;
                }
            }
        }

        private void PerformSearch(string word)
        {
            if (string.IsNullOrEmpty(word)) return;

            if (DataContext is DictionaryViewModel vm)
            {

                vm.SelectedSuggestion = word;

                vm.SuggestionList.Clear();

                vm.SearchText = word;

                if (vm.SearchCommand.CanExecute(null))
                {
                    vm.SearchCommand.Execute(null);
                }
            }

            SearchTextBox.Text = word;
            SearchTextBox.CaretIndex = SearchTextBox.Text.Length;
            SearchTextBox.Focus();
        }
    }
}