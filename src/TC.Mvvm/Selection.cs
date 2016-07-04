using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

    /// <summary>
    /// Represents a collection of selectable objects (<see cref="ISelectable"/>). For convenience in WPF applications, this class is a descendant of
    /// <see cref="ObservableCollection{T}"/>.
    /// </summary>
    public class Selection : ObservableCollection<ISelectable>
    {

        /// <summary>
        /// Initializes a new instance of <see cref="Selection"/> as an empty selection.
        /// </summary>
        public Selection()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Selection"/> with the given set of selectable items.
        /// </summary>
        /// <param name="items"></param>
        public Selection(IEnumerable<ISelectable> items)
            : base(items)
        {
            foreach(ISelectable item in items)
                item.IsSelected = true;
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            foreach(ISelectable node in this)
                node.IsSelected = false;

            base.ClearItems();
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, ISelectable item)
        {
            item.IsSelected = true;

            base.InsertItem(index, item);
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            this[index].IsSelected = false;

            base.RemoveItem(index);
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, ISelectable item)
        {
            this[index].IsSelected = false;
            item.IsSelected = true;

            base.SetItem(index, item);
        }

    }

}
