using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TC.Mvvm
{

	public class BaseModelObj : INotifyPropertyChanged
	{

		private static int nextRid = 0;
		private int rid = nextRid++;

		public int Rid
		{
			get { return rid; }
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if(PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual bool SetValue<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = null, bool suppressModified = false)
		{
			EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

			if(!equalityComparer.Equals(backingField, newValue))
			{
				backingField = newValue;
				OnPropertyChanged(propertyName);
				if(!suppressModified)
					OnModified();
				return true;
			}

			return false;
		}

		protected virtual void OnModified()
		{
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

	}

}
