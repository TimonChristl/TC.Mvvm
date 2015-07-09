using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

	public interface IIdIndexable<TItem, TId>
	{

		TItem this[TId id] { get; set; }

	}

}
