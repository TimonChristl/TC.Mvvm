using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

	public abstract class ObservableCollectionWithOwnerIdIndexable<TItem, TOwner, TId> : ObservableCollectionWithOwner<TItem, TOwner>, IIdIndexable<TItem, TId>
		where TItem : class, IHasOwner<TOwner>
		where TOwner : class
	{

		public ObservableCollectionWithOwnerIdIndexable(TOwner owner)
			: base(owner)
		{
		}

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

		protected abstract TId GetItemId(TItem item);

	}

}
