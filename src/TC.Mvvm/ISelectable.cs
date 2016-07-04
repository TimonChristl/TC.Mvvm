using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

    /// <summary>
    /// Defines that an object supports being part of a selection. A selectable object has a kind, which is useful when there are different
    /// kinds of selectable objects that must be distinguished.
    /// </summary>
    public interface ISelectable
    {

        /// <summary>
        /// Defines whether the selectable is selected or not.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Defines which kind of selectable the selectable is.
        /// </summary>
        object Kind { get; }

    }

}
