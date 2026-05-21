using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace THUVIENZ.Commands
{
    /// <summary>
    /// Asynchronous ICommand with optional canExecute predicate and CommandManager integration for CanExecuteChanged.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();

        public async void Execute(object? parameter)
        {
            await _execute().ConfigureAwait(false);
        }
    }
}
