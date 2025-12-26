using System;
using System.Windows.Input;

namespace LexiScan.App.Commands
{
    // Lớp triển khai ICommand cơ bản để hỗ trợ kiến trúc MVVM.
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;

        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Thực thi Command 
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        // thông báo cho WPF UI khi trạng thái CanExecute thay đổi.
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}