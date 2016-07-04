using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TC.Mvvm
{

    /// <summary>
    /// An <see cref="ICommand"/> implementation that uses delegates for <see cref="ICommand.CanExecute(object)"/> and
    /// <see cref="ICommand.Execute(object)"/>.
    /// </summary>
	public class DelegateCommand : ICommand
    {

        private Func<object, bool> canExecute;
        private Action<object> execute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/>.
        /// </summary>
        /// <param name="canExecute"></param>
        /// <param name="execute"></param>
        public DelegateCommand(Func<object, bool> canExecute, Action<object> execute)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        /// <summary>
        /// Fires the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void FireCanExecuteChanged()
        {
            if(CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        #region ICommand Members

        /// <inheritdoc/>
        public bool CanExecute(object parameter)
        {
            return canExecute != null ? canExecute(parameter) : false;
        }

        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;

        /// <inheritdoc/>
        public void Execute(object parameter)
        {
            if(execute != null)
                execute(parameter);
        }

        #endregion

    }

}
