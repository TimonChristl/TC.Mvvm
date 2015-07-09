using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TC.Mvvm
{

	public class DelegateCommand : ICommand
	{

		private Func<object, bool> canExecute;
		private Action<object> execute;

		public DelegateCommand(Func<object, bool> canExecute, Action<object> execute)
		{
			this.canExecute = canExecute;
			this.execute = execute;
		}

		public void FireCanExecuteChanged()
		{
			if(CanExecuteChanged != null)
				CanExecuteChanged(this, EventArgs.Empty);
		}

		#region ICommand Members

		public bool CanExecute(object parameter)
		{
			return canExecute != null ? canExecute(parameter) : false;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			if(execute != null)
				execute(parameter);
		}

		#endregion

	}

}
