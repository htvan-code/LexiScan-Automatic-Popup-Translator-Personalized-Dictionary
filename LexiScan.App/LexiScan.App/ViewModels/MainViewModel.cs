// File: ViewModels/MainViewModel.cs

using System.Windows.Input;
using LexiScan.App.Commands;
using System;
using System.Collections.Generic; // Cần thiết cho Dictionary

namespace LexiScan.App.ViewModels
{
    // Giả định BaseViewModel đã tồn tại và xử lý INotifyPropertyChanged
    public class MainViewModel : BaseViewModel
    {
        // Khai báo các instance Singleton cho các View chính
        // Sử dụng khởi tạo mặc định cho các ViewModel giả định
        private readonly DictionaryViewModel _dictionaryVM = new();
        private readonly PersonalDictionaryViewModel _personalDictionaryVM = new();
        private readonly HistoryViewModel _historyVM = new();
        private readonly TranslationViewModel _translationVM = new();
        private readonly SettingsViewModel _settingsVM = new();

        // Thuộc tính giữ View hiện tại đang được hiển thị.
        // Khắc phục CS8618: Khởi tạo CurrentView trong Constructor
        private BaseViewModel _currentView;

        // Bổ sung CurrentView setter/getter
        public BaseViewModel CurrentView
        {
            get => _currentView;
            set
            {
                // Chỉ set nếu giá trị khác nhau để tránh vòng lặp/gọi OnPropertyChanged không cần thiết
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NavigateCommand { get; } // Dùng `get;` để tuân thủ ReadOnly

        public MainViewModel()
        {
            // Khởi tạo Command.
            NavigateCommand = new RelayCommand(Navigate);

            // **THIẾT LẬP HOME/DICTIONARY VIEW LÀ MẶC ĐỊNH**
            // Sử dụng instance đã tạo
            _currentView = _dictionaryVM; // Khởi tạo trường backing field
        }

        // Phương thức xử lý logic chuyển đổi View
        // Sử dụng dấu gạch dưới `_` cho tham số không dùng
        private void Navigate(object? parameter)
        {
            string? viewName = parameter as string;

            if (string.IsNullOrEmpty(viewName)) return; // Trả về nếu tham số rỗng hoặc null

            // Dựa vào CommandParameter để chuyển View
            CurrentView = viewName switch
            {
                // Home button, theo yêu cầu là DictionaryView (Tái sử dụng instance)
                "Home" => _dictionaryVM,
                // Từ điển cá nhân (Tái sử dụng instance)
                "Dictionary" => _personalDictionaryVM,
                // Lịch sử (Tái sử dụng instance)
                "History" => _historyVM,
                // Dịch văn bản (Tái sử dụng instance)
                "Translation" => _translationVM,
                // Cài đặt (Tái sử dụng instance)
                "Settings" => _settingsVM,
                _ => CurrentView // Giữ nguyên View nếu không khớp
            };
        }
    }

    // **Các lớp ViewModel giả định phải kế thừa từ BaseViewModel**
    // LƯU Ý: Đã xóa các ViewModel giả định trùng lặp với file HistoryViewModel.cs
    // Chỉ giữ lại các ViewModel chưa có file riêng
    public class DictionaryViewModel : BaseViewModel { }
    public class PersonalDictionaryViewModel : BaseViewModel { }
    public class TranslationViewModel : BaseViewModel { }
    // public class HistoryViewModel : BaseViewModel { } // Đã có file riêng
    // public class SettingsViewModel : BaseViewModel { } // Đã có file riêng
}