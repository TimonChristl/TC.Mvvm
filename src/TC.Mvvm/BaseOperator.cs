using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

    /// <summary>
    /// Base class for operators to be used with <see cref="BaseOperatorManager{TContext, TMemento}"/>. An operator
    /// is a well-defined and reversible change to the data structures of an application.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
	public abstract class BaseOperator<TContext>
    {

        #region IOperator Members

        /// <summary>
        /// Prepares the operator for execution. This method is only called once before the first call to <see cref="Apply(TContext)"/>,
        /// and can be used to perform one-time setup of the change represented by the operator. If you need to interact with the user,
        /// (for example, show a dialog to the user), either put it in this method, or outside the operator.
        /// </summary>
        /// <param name="context">Context of the operator manager that is passed to the operator</param>
        public abstract void Prepare(TContext context);

        /// <summary>
        /// Applies the changes that are represented by this operator. This method is called when the
        /// operator is first added to the operator manager, and by <see cref="BaseOperatorManager{TContext, TMemento}.Redo"/>.
        /// It is expected that no user interaction is performed in this method.
        /// </summary>
        /// <param name="context">Context of the operator manager that is passed to the operator</param>
		public abstract void Apply(TContext context);

        /// <summary>
        /// Unapplies (reverts) the changes that are represented by this operator. This method is called by <see cref="BaseOperatorManager{TContext, TMemento}.Undo"/>.
        /// It is expected that no user interaction is performed in this method.
        /// </summary>
        /// <param name="context">Context of the operator manager that is passed to the operator</param>
		public abstract void Unapply(TContext context);

        #endregion

    }

}
