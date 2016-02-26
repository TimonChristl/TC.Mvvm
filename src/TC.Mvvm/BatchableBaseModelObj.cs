using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

    public class PropertyValueChangedEventArgs : EventArgs
    {

        private string propertyName;

        public PropertyValueChangedEventArgs(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public string PropertyName
        {
            get { return propertyName; }
        }

    }

    /// <summary>
    /// An update batch is used to defer raising <see cref="INotifyPropertyChanged.PropertyChanged"/> for all batchable base model objects registered with the update batch until
    /// the batch is disposed. The events are fired in the order they would normally have been fired, except that multiple firings for the same model object and the same
    /// property name are collapsed to only the last event fire.
    /// </summary>
    /// <remarks>
    /// If model objects rely on the <see cref="INotifyPropertyChanged.PropertyChanged"/> event to cause other property changes in the same model object or in other model objects,
    /// and it is important that raising the event is not deferred until the update batch is disposed, or that multiple event firings are not collapsed to only the last one,
    /// use the <see cref="BatchableBaseModelObj.PropertyValueChanged"/> event instead.
    /// This event is always fired when a property value is changed using <see cref="BaseModelObj.SetValue{T}(ref T, T, string, bool)"/>, even in an update batch.
    /// </remarks>
    public class UpdateBatch : IDisposable
    {

        private class PendingPropertyChangedEvent
        {
            public BatchableBaseModelObj BatchableBaseModelObj;
            public string PropertyName;
            public int SequenceNumber;
        }

        private HashSet<BatchableBaseModelObj> registeredBatchableBaseModelObjs = new HashSet<BatchableBaseModelObj>();
        private List<PendingPropertyChangedEvent> pendingPropertyChangedEvents = new List<PendingPropertyChangedEvent>();
        private int nextSequenceNumber = 0;

        /// <summary>
        /// Registers the batchable base model object <paramref name="batchableBaseModelObj"/> to this update batch.
        /// If the model object is already registered to his update batch, this method does nothing.
        /// If the model object is already registered to a different update batch, the exception <see cref="ArgumentException"/> is raised.
        /// </summary>
        /// <param name="batchableBaseModelObj"></param>
        public void Register(BatchableBaseModelObj batchableBaseModelObj)
        {
            if(batchableBaseModelObj == null)
                throw new ArgumentNullException(nameof(batchableBaseModelObj));

            if(!registeredBatchableBaseModelObjs.Contains(batchableBaseModelObj))
            {
                if(batchableBaseModelObj.UpdateBatch != null)
                    throw new ArgumentException("BatchableBaseModelObject must not be already registered to an UpdateBatch", nameof(batchableBaseModelObj));

                registeredBatchableBaseModelObjs.Add(batchableBaseModelObj);
                batchableBaseModelObj.UpdateBatch = this;
            }
        }

        /// <summary>
        /// Unregisters the batchable base model object <paramref name="batchableBaseModelObj"/> from this update batch. If the
        /// model object is not registered, this method does nothing.
        /// Unregistering a model object causes all pending <see cref="INotifyPropertyChanged.PropertyChanged"/> event firings for the
        /// model object to be performed immediately instead of when the update batch is disposed.
        /// </summary>
        /// <param name="batchableBaseModelObj"></param>
        public void Unregister(BatchableBaseModelObj batchableBaseModelObj)
        {
            if(batchableBaseModelObj == null)
                throw new ArgumentNullException(nameof(batchableBaseModelObj));

            if(registeredBatchableBaseModelObjs.Contains(batchableBaseModelObj))
            {
                FirePendingPropertyChangedEvents(batchableBaseModelObj);
                registeredBatchableBaseModelObjs.Remove(batchableBaseModelObj);
                batchableBaseModelObj.UpdateBatch = null;
            }
        }

        private void FirePendingPropertyChangedEvents()
        {
            foreach(var pendingPropertyChangedEvent in pendingPropertyChangedEvents.OrderBy(ppce => ppce.SequenceNumber))
            {
                pendingPropertyChangedEvent.BatchableBaseModelObj.UpdateBatch = null;
                pendingPropertyChangedEvent.BatchableBaseModelObj.FirePropertyChangedEvent(pendingPropertyChangedEvent.PropertyName);
            }

            pendingPropertyChangedEvents.Clear();
        }

        private void FirePendingPropertyChangedEvents(BatchableBaseModelObj batchableBaseModelObj)
        {
            foreach(var pendingPropertyChangedEvent in pendingPropertyChangedEvents.OrderBy(ppce => ppce.SequenceNumber).Where(ppce => ppce.BatchableBaseModelObj == batchableBaseModelObj).ToArray())
            {
                pendingPropertyChangedEvents.Remove(pendingPropertyChangedEvent);
                pendingPropertyChangedEvent.BatchableBaseModelObj.FirePropertyChangedEvent(pendingPropertyChangedEvent.PropertyName);
            }
        }

        internal void RegisterPendingPropertyChangedEvent(BatchableBaseModelObj batchableBaseModelObj, string propertyName)
        {
            pendingPropertyChangedEvents.RemoveAll(ppce => ppce.BatchableBaseModelObj == batchableBaseModelObj && ppce.PropertyName == propertyName);

            pendingPropertyChangedEvents.Add(new PendingPropertyChangedEvent
            {
                BatchableBaseModelObj = batchableBaseModelObj,
                PropertyName = propertyName,
                SequenceNumber = nextSequenceNumber++,
            });
        }

        internal bool IsRegistered(BatchableBaseModelObj batchableBaseModelObj)
        {
            return registeredBatchableBaseModelObjs.Contains(batchableBaseModelObj);
        }

        #region IDisposable implementation

        private bool isDisposed = false;

        ~UpdateBatch()
        {
            if(!isDisposed)
                DisposeCore(false);
        }

        public void Dispose()
        {
            if(!isDisposed)
            {
                DisposeCore(true);
                isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private void DisposeCore(bool disposing)
        {
            if(disposing)
            {
                FirePendingPropertyChangedEvents();
            }
        }

        #endregion

    }

    /// <summary>
    /// A variation of <see cref="BaseModelObj"/> that supports update batches (see <see cref="TC.Mvvm.UpdateBatch"/>).
    /// </summary>
    public class BatchableBaseModelObj : BaseModelObj
    {

        private UpdateBatch updateBatch;

        internal UpdateBatch UpdateBatch
        {
            get { return updateBatch; }
            set { updateBatch = value; }
        }

        /// <summary>
        /// Fires the <see cref="PropertyValueChanged"/> event for property <paramref name="propertyName"/>. This event is always fired for each and every property change, whereas
        /// <see cref="BaseModelObj.PropertyChanged"/> is deferred when the model object is registered in an update batch.
        /// </summary>
        /// <remarks>
        /// Argument <paramref name="propertyName"/> is annotated with <see cref="CallerMemberNameAttribute"/> and does therefore not have to be specified manually in most cases.
        /// </remarks>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyValueChanged([CallerMemberName] string propertyName = null)
        {
            if(PropertyValueChanged != null)
                PropertyValueChanged(this, new PropertyValueChangedEventArgs(propertyName));
        }

        internal void FirePropertyChangedEvent(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// If this model object is registered with an <see cref="UpdateBatch"/>, registers a pending <see cref="BaseModelObj.PropertyChanged"/> event firing with that <see cref="UpdateBatch"/>,
        /// otherwise immediately fires the <see cref="BaseModelObj.PropertyChanged"/> event for property <paramref name="propertyName"/> by calling the implementation in the base class.
        /// </summary>
        /// <remarks>
        /// Argument <paramref name="propertyName"/> is annotated with <see cref="CallerMemberNameAttribute"/> and does therefore not have to be specified manually in most cases.
        /// </remarks>
        /// <param name="propertyName"></param>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(updateBatch != null && updateBatch.IsRegistered(this))
                updateBatch.RegisterPendingPropertyChangedEvent(this, propertyName);
            else
                base.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Compares <paramref name="backingField"/> against <paramref name="newValue"/> using <see cref="EqualityComparer{T}.Default"/>.
        /// If the two values are different, <paramref name="newValue"/> is assigned to <paramref name="backingField"/>, then
        /// <see cref="OnPropertyValueChanged(string)"/> and <see cref="OnPropertyChanged(string)"/> are called, unless <paramref name="propertyName"/> is <c>null</c>,
        /// and <see cref="BaseModelObj.OnModified"/> is called, unless <paramref name="suppressModified"/> is <c>true</c>.
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
        protected override bool SetValue<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = null, bool suppressModified = false)
        {
            EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

            if(!equalityComparer.Equals(backingField, newValue))
            {
                backingField = newValue;
                if(propertyName != null)
                {
                    OnPropertyValueChanged(propertyName);
                    OnPropertyChanged(propertyName);
                }

                if(!suppressModified)
                    OnModified();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Fired by <see cref="OnPropertyValueChanged(string)"/> after a property value was changed.
        /// </summary>
        public event EventHandler<PropertyValueChangedEventArgs> PropertyValueChanged;

    }

}
