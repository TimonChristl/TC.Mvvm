using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

	public class Selection : ObservableCollection<ISelectable>
	{

		public Selection()
		{
		}

		public Selection(IEnumerable<ISelectable> items)
			: base(items)
		{
			foreach(ISelectable item in items)
				item.IsSelected = true;
		}

		protected override void ClearItems()
		{
			foreach(ISelectable node in this)
				node.IsSelected = false;

			base.ClearItems();
		}

		protected override void InsertItem(int index, ISelectable item)
		{
			item.IsSelected = true;

			base.InsertItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			this[index].IsSelected = false;

			base.RemoveItem(index);
		}

		protected override void SetItem(int index, ISelectable item)
		{
			this[index].IsSelected = false;
			item.IsSelected = true;

			base.SetItem(index, item);
		}

	}

}
