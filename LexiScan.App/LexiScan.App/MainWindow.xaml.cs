// File: MainWindow.xaml.cs

using System.Windows;
using LexiScan.App.ViewModels; 
namespace LexiScan.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
    }
}