// File: Commands/RelayCommand.cs

using System;
using System.Windows.Input;

// Đặt trong namespace riêng biệt để dễ quản lý.
namespace LexiScan.App.Commands
{
    public class RelayCommand : ICommand
    {
        // Sử dụng object? để khớp với ICommand interface và xử lý nullability
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute; // Thêm '?' vì nó có thể là null

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Khắc phục CS8376: Sử dụng object?
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Khắc phục CS8376: Sử dụng object?
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        // Khắc phục CS8612: Thêm '?' cho event CanExecuteChanged
        public event EventHandler? CanExecuteChanged
        {
            // Sử dụng CommandManager để tích hợp với WPF UI
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}