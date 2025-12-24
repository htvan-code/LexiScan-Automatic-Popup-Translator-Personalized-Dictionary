using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using LexiScanUI.ViewModels;

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
