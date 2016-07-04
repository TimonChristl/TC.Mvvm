using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

    /// <summary>
    /// An observable collection where each item implements <see cref="IHasOwner{TOwner}"/>. The
    /// owners of the items in the collection are maintained automatically when items are added and removed.
    /// </summary>
    /// <typeparam name="TItem">Item type</typeparam>
    /// <typeparam name="TOwner">Owner type</typeparam>
    public class ObservableCollectionWithOwner<TItem, TOwner> : ObservableCollection<TItem>
        where TItem : class, IHasOwner<TOwner>
        where TOwner : class
    {

        private TOwner owner;

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableCollectionWithOwner{TItem, TOwner}"/>.
        /// </summary>
        /// <param name="owner">The owner object that owns the items in this collection</param>
        public ObservableCollectionWithOwner(TOwner owner)
        {
            this.owner = owner;
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            foreach(TItem item in this)
                item.Owner = null;
            base.ClearItems();
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, TItem item)
        {
            item.Owner = owner;
            base.InsertItem(index, item);
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            TItem item = this[index];
            base.RemoveItem(index);
            item.Owner = null;
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, TItem item)
        {
            TItem oldItem = this[index];
            item.Owner = owner;
            base.SetItem(index, item);
            oldItem.Owner = null;
        }

    }

}
