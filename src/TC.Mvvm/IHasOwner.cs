using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

	public interface IHasOwner<TOwner>
		where TOwner : class
	{

		TOwner Owner { get; set; }

	}

}
