using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TC.Mvvm
{

    /// <summary>
    /// Base class for models and view models. Implements <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    /// <example>
    /// Use the following pattern for properties to get property change events:
    /// <code>
    /// private int field;
    /// 
    /// public int Property
    /// {
    ///     get { return field;}
    ///     set { SetValue(ref field, value); }
    /// }
    /// </code>
    /// </example>
	public class BaseModelObj : INotifyPropertyChanged
    {

        private static int nextRid = 0;
        private int rid = nextRid++;

        /// <summary>
        /// The runtime id of this model object. Each new model object gets a new, a sequentially increasing runtime id. This property can be useful
        /// to identify models during debugging, and to distinguish two otherwise identical copies of a given model.
        /// </summary>
		public int Rid
        {
            get { return rid; }
        }

        /// <summary>
        /// Fires the <see cref="PropertyChanged"/> event for property <paramref name="propertyName"/>.
        /// </summary>
        /// <remarks>
        /// Argument <paramref name="propertyName"/> is annotated with <see cref="CallerMemberNameAttribute"/> and does therefore not have to be specified manually in most cases.
        /// </remarks>
        /// <param name="propertyName"></param>
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Compares <paramref name="backingField"/> against <paramref name="newValue"/> using <see cref="EqualityComparer{T}.Default"/>.
        /// If the two values are different, <paramref name="newValue"/> is assigned to <paramref name="backingField"/>, then
        /// <see cref="OnPropertyChanged(string)"/> is called, unless <paramref name="propertyName"/> is <c>null</c>,
        /// and <see cref="OnModified"/> is called, unless <paramref name="suppressModified"/> is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Argument <paramref name="propertyName"/> is annotated with <see cref="CallerMemberNameAttribute"/> and does therefore not have to be specified manually in most cases.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="backingField"></param>
        /// <param name="newValue"></param>
        /// <param name="propertyName"></param>
        /// <param name="suppressModified"></param>
        /// <returns></returns>
		protected virtual bool SetValue<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = null, bool suppressModified = false)
        {
            EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

            if(!equalityComparer.Equals(backingField, newValue))
            {
                backingField = newValue;
                if(propertyName != null)
                    OnPropertyChanged(propertyName);
                if(!suppressModified)
                    OnModified();
                return true;
            }

            return false;
        }

        /// <summary>
        /// When overriden in a derived class, signals that the object changed as a result of calling <see cref="SetValue{T}(ref T, T, string, bool)"/>. The base
        /// class implementation does nothing.
        /// </summary>
		protected virtual void OnModified()
        {
        }

        #region INotifyPropertyChanged Members

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }

}
