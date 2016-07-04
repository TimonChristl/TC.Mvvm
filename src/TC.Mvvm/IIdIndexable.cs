using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

    /// <summary>
    /// Defines that this object supports access to child objects of type <typeparamref name="TItem"/> by IDs of type <typeparamref name="TId"/>.
    /// Each child object is expected to have a unique ID within its parent.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public interface IIdIndexable<TItem, TId>
    {

        /// <summary>
        /// Provides access to child objects by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TItem this[TId id] { get; set; }

    }

}
