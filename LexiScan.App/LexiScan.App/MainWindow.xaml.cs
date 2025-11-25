using System.Windows.Controls;
using System.Windows.Input;
using LexiScan.App.Views; // Đảm bảo include namespace Views

namespace LexiScan.App.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private UserControl _currentView;

        public UserControl CurrentView
        {
            get { return _currentView; }
            set { SetProperty(ref _currentView, value); }
        }

        public ICommand NavigateCommand { get; set; }

        public MainViewModel()
        {
            // THIẾT LẬP TRANG CHỦ MẶC ĐỊNH KHI ỨNG DỤNG KHỞI ĐỘNG
            NavigateCommand = new RelayCommand(ExecuteNavigate);
            CurrentView = new DictionaryView();
        }

        private void ExecuteNavigate(object parameter)
        {
            string viewName = parameter as string;

            if (viewName == null) return;

            // Xử lý chuyển View dựa trên CommandParameter
            switch (viewName)
            {
                case "Home":
                    CurrentView = new DictionaryView();
                    break;
                case "Dictionary":
                    CurrentView = new PersonalDictionaryView();
                    break;
                case "History":
                    CurrentView = new HistoryView();
                    break;
                case "Translation":
                    CurrentView = new TranslationView();
                    break;
                case "Settings":
                    CurrentView = new SettingsView();
                    break;
            }
        }
    }
}