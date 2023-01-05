using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Common.Foundation
{
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;   // 실행할 수 있는지 (이미 실행중이지는 않는지)
        private readonly Action<object> _execute;         // 실행

        public event EventHandler CanExecuteChanged;      // 실행에 대한 이벤트 핸들러

        public DelegateCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
