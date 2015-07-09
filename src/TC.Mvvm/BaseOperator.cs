using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

	public abstract class BaseOperator<TContext>
	{

		#region IOperator Members

		public abstract void Prepare(TContext context);

		public abstract void Apply(TContext context);

		public abstract void Unapply(TContext context);

		#endregion

	}

}
