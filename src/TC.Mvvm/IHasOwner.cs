using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

    /// <summary>
    /// Defines that the object has an owner of type <typeparamref name="TOwner"/>.
    /// </summary>
    /// <typeparam name="TOwner"></typeparam>
    public interface IHasOwner<TOwner>
        where TOwner : class
    {

        /// <summary>
        /// Owner of this object.
        /// </summary>
        TOwner Owner { get; set; }

    }

}
