using System;
using System.Windows.Input;

namespace LexiScan.App.Commands
{
    // Lớp triển khai ICommand cơ bản để hỗ trợ kiến trúc MVVM.
    public class RelayCommand : ICommand
    {
        // Action chứa logic sẽ được thực thi khi Command được gọi (Execute).
        private readonly Action<object?> _execute;

        // Func chứa logic kiểm tra xem Command có thể được thực thi hay không (CanExecute).
        private readonly Func<object?, bool>? _canExecute;

        // Constructor mặc định, cho phép canExecute là null (luôn luôn có thể thực thi)
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Kiểm tra xem Command có thể được thực thi hay không
        public bool CanExecute(object? parameter)
        {
            // Nếu _canExecute là null, luôn trả về true. Ngược lại, gọi hàm kiểm tra.
            return _canExecute == null || _canExecute(parameter);
        }

        // Thực thi Command (gọi Action _execute)
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        // Sự kiện này thông báo cho WPF UI khi trạng thái CanExecute thay đổi.
        public event EventHandler? CanExecuteChanged
        {
            // Sử dụng CommandManager để tích hợp với WPF UI
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}