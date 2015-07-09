using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

	public class ObservableCollectionWithOwner<TItem, TOwner> : ObservableCollection<TItem>
		where TItem : class, IHasOwner<TOwner>
		where TOwner : class
	{

		private TOwner owner;

		public ObservableCollectionWithOwner(TOwner owner)
		{
			this.owner = owner;
		}

		protected override void ClearItems()
		{
			foreach(TItem item in this)
				item.Owner = null;
			base.ClearItems();
		}

		protected override void InsertItem(int index, TItem item)
		{
			item.Owner = owner;
			base.InsertItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			TItem item = this[index];
			base.RemoveItem(index);
			item.Owner = null;
		}

		protected override void SetItem(int index, TItem item)
		{
			TItem oldItem = this[index];
			item.Owner = owner;
			base.SetItem(index, item);
			oldItem.Owner = null;
		}

	}

}
