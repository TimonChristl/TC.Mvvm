using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

    /// <summary>
    /// An observable collection where each item implements <see cref="IHasOwner{TOwner}"/> and where the items are indexable
    /// by ID. The owners of the items in the collection are maintained automatically when items are added and removed.
    /// </summary>
    /// <typeparam name="TItem">Item type</typeparam>
    /// <typeparam name="TOwner">Owner type</typeparam>
    /// <typeparam name="TId">Item ID type</typeparam>
	public abstract class ObservableCollectionWithOwnerIdIndexable<TItem, TOwner, TId> : ObservableCollectionWithOwner<TItem, TOwner>, IIdIndexable<TItem, TId>
        where TItem : class, IHasOwner<TOwner>
        where TOwner : class
    {

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableCollectionWithOwner{TItem, TOwner}"/>.
        /// </summary>
        /// <param name="owner">The owner object that owns the items in this collection</param>
        public ObservableCollectionWithOwnerIdIndexable(TOwner owner)
            : base(owner)
        {
        }

        /// <inheritdoc/>
        public TItem this[TId id]
        {
            get
            {
                EqualityComparer<TId> equalityComparer = EqualityComparer<TId>.Default;

                for(int i = 0; i < this.Count; i++)
                {
                    TItem item = this[i];
                    if(equalityComparer.Equals(GetItemId(item), id))
                        return this[i];
                }

                return null;
            }
            set
            {
                EqualityComparer<TId> equalityComparer = EqualityComparer<TId>.Default;

                for(int i = 0; i < this.Count; i++)
                {
                    TItem item = this[i];
                    if(equalityComparer.Equals(GetItemId(item), id))
                    {
                        this[i] = value;
                        return;
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected abstract TId GetItemId(TItem item);

    }

}
