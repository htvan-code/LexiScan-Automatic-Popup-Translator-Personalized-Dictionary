// File: ViewModels/BaseViewModel.cs

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LexiScan.App.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        // Khắc phục CS8618/CS8612: Thêm '?' để cho phép null
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            // Sử dụng toán tử null-conditional (?. ) là đủ an toàn
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}