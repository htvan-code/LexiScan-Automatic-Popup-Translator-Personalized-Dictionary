using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace LexiScanUI
{
    public partial class SelectableWord : ObservableObject
    {
        [ObservableProperty]
        private string _text;

        public ICommand? ClickCommand { get; }

        public SelectableWord(string text, ICommand? command = null)
        {
            Text = text;
            ClickCommand = command;
        }
    }
}
