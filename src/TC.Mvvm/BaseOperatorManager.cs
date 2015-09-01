using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

	/// <summary>
	/// Base implementation of an operator manager, which manages two stacks of steps, where each step
	/// consists of at least one operator. One stack is for applied steps, the other for unapplied steps.
	/// A context is passed to all operators, which can be used to pass models or view models to the operators.
	/// </summary>
	/// <remarks>
	/// Two memento objects are stored for each step, which can be used to store additional data that is
	/// not directly accessible from the context but should be saved and restored when steps are undone or redone.
	/// The "before" memento of a step is restored when undoing a step, while the "after" memento of a step is restored
	/// when redoing a step.
	/// </remarks>
	/// <typeparam name="TContext"></typeparam>
	/// <typeparam name="TMemento"></typeparam>
	public abstract class BaseOperatorManager<TContext, TMemento> : INotifyPropertyChanged
		where TMemento : class
	{

		private class Step
		{
            public string Description;
			public TMemento Before;
			public BaseOperator<TContext>[] Operators;
			public TMemento After;

			public Step(string description, BaseOperator<TContext>[] operators)
			{
                this.Description = description;
				this.Operators = operators;
			}
		}

		private TContext context;
		private Stack<Step> appliedSteps = new Stack<Step>();
		private Stack<Step> unappliedSteps = new Stack<Step>();

		/// <summary>
		/// Creates a new base operator manager for the given context.
		/// </summary>
		/// <param name="context"></param>
		public BaseOperatorManager(TContext context)
		{
			this.context = context;
			Clear();
		}

		/// <summary>
		/// Clears both the stack of applied steps and the stack of unapplied steps.
		/// </summary>
		public void Clear()
		{
			this.appliedSteps.Clear();
			this.unappliedSteps.Clear();

			OnPropertyChanged("CanUndo");
			OnPropertyChanged("CanRedo");
			OnChanged();
		}

        /// <summary>
        /// Add an undo step that encapsulates the supplied list of operators. The "before" memento for the
        /// step is created by calling <see cref="CreateMemento()"/>.
        /// The new step is pushed onto the stack of applied steps, and the stack of unapplied steps is cleared.
        /// </summary>
        /// <remarks>
        /// Use this overload when the "before" memento should reflect the current state.
        /// If no operators are supplied, this method does nothing.
        /// </remarks>
        /// <param name="operators"></param>
        public void Add(params BaseOperator<TContext>[] operators)
        {
            AddCore(null, null, operators);
        }

        /// <summary>
        /// Add an undo step that encapsulates the supplied list of operators. The "before" memento for the
        /// step is set to the supplied memento.
        /// The new step is pushed onto the stack of applied steps, and the stack of unapplied steps is cleared.
        /// </summary>
        /// <remarks>
        /// Use this overload when the "before" memento needs to reflect an earlier state than the one at the
        /// time this method is called.
        /// If no operators are supplied, this method does nothing.
        /// </remarks>
        /// <param name="beforeMemento"></param>
        /// <param name="operators"></param>
        public void Add(TMemento beforeMemento, params BaseOperator<TContext>[] operators)
        {
            AddCore(null, CreateMemento(context), operators);
        }

        /// <summary>
        /// Add an undo step that encapsulates the supplied list of operators. The "before" memento for the
        /// step is created by calling <see cref="CreateMemento()"/>.
        /// The new step is pushed onto the stack of applied steps, and the stack of unapplied steps is cleared.
        /// </summary>
        /// <remarks>
        /// Use this overload when the "before" memento should reflect the current state.
        /// If no operators are supplied, this method does nothing.
        /// </remarks>
        /// <param name="description"></param>
        /// <param name="operators"></param>
        public void Add(string description, params BaseOperator<TContext>[] operators)
		{
			AddCore(description, null, operators);
		}

        /// <summary>
        /// Add an undo step that encapsulates the supplied list of operators. The "before" memento for the
        /// step is set to the supplied memento.
        /// The new step is pushed onto the stack of applied steps, and the stack of unapplied steps is cleared.
        /// </summary>
        /// <remarks>
        /// Use this overload when the "before" memento needs to reflect an earlier state than the one at the
        /// time this method is called.
        /// If no operators are supplied, this method does nothing.
        /// </remarks>
        /// <param name="description"></param>
        /// <param name="beforeMemento"></param>
        /// <param name="operators"></param>
        public void Add(string description, TMemento beforeMemento, params BaseOperator<TContext>[] operators)
		{
			AddCore(description, CreateMemento(context), operators);
		}

		private void AddCore(string description, TMemento beforeMemento, BaseOperator<TContext>[] operators)
		{
			if(operators.Length == 0)
				return;

			Step step = new Step(description, operators);

			step.Before = beforeMemento;

			this.appliedSteps.Push(step);
			this.unappliedSteps.Clear();

			Stack<BaseOperator<TContext>> appliedOperators = new Stack<BaseOperator<TContext>>();

			foreach(BaseOperator<TContext> op in operators)
			{
				try
				{
					op.Prepare(context);
					op.Apply(context);
					appliedOperators.Push(op);
				}
				catch
				{
					while(appliedOperators.Count > 0)
						appliedOperators.Pop().Unapply(context);
					throw;
				}
			}

			step.After = CreateMemento(context);

			OnPropertyChanged("CanUndo");
			OnPropertyChanged("CanRedo");
			OnChanged();
		}

		/// <summary>
		/// Performs an undo by popping a step from the stack of applied steps, unapplying its
		/// list of operators in reverse order, pushing the step onto the stack of unapplied steps,
		/// and restoring the "before" memento of the step.
		/// </summary>
		public void Undo()
		{
			Step step = this.appliedSteps.Pop();
			this.unappliedSteps.Push(step);

			for(int i = step.Operators.Length - 1; i >= 0; i--)
				step.Operators[i].Unapply(context);

			RestoreMemento(context, step.Before);

			OnPropertyChanged("CanUndo");
			OnPropertyChanged("CanRedo");
			OnChanged();
		}

		/// <summary>
		/// Performs a redo by popping a step from the stack of unapplied steps, applying its
		/// list of operators in forward order, pushing the step onto the stack of applied steps,
		/// and restoring the "after" memento of the step.
		/// </summary>
		public void Redo()
		{
			Step step = this.unappliedSteps.Pop();
			this.appliedSteps.Push(step);

			for(int i = 0; i < step.Operators.Length; i++)
				step.Operators[i].Apply(context);

			RestoreMemento(context, step.After);

			OnPropertyChanged("CanUndo");
			OnPropertyChanged("CanRedo");
			OnChanged();
		}

		/// <summary>
		/// Create a new memento by invoking the protected overload <see cref="CreateMemento(TContext)"/>.
		/// </summary>
		/// <returns></returns>
		public TMemento CreateMemento()
		{
			return CreateMemento(context);
		}

		/// <summary>
		/// Create a new memento. This method must be overridden in a non-abstract operator manager.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		protected abstract TMemento CreateMemento(TContext context);

		/// <summary>
		/// Restore the supplied memento. This method must be overridden in a non-abstract operator manager.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="memento"></param>
		protected abstract void RestoreMemento(TContext context, TMemento memento);

		/// <summary>
		/// Gets whether undo is possible. Undo is possible if the stack of applied steps is non-empty.
		/// </summary>
		public bool CanUndo
		{
			get { return appliedSteps.Count > 0; }
		}

        /// <summary>
        /// Gets whether redo is possible. Redo is possible if the stack of unapplied steps is non-empty.
        /// </summary>
        public bool CanRedo
		{
			get { return unappliedSteps.Count > 0; }
		}

        /// <summary>
        /// Description of the step that will be unapplied if <see cref="Undo"/> is be called, or <c>null</c>,
        /// if no such step exists.
        /// </summary>
        /// <returns></returns>
        public string UndoDescription
        {
            get
            {
                if(appliedSteps.Count == 0)
                    return null;

                var step = appliedSteps.Peek();
                return step.Description;
            }
        }

        /// <summary>
        /// Description of the step that will be applied if <see cref="Redo"/> is be called, or <c>null</c>,
        /// if no such step exists.
        /// </summary>
        /// <returns></returns>
        public string RedoDescription
        {
            get
            {
                if(unappliedSteps.Count == 0)
                    return null;

                var step = unappliedSteps.Peek();
                return step.Description;
            }
        }

        /// <summary>
        /// Fires the <see cref="Changed"/> event.
        /// </summary>
        protected virtual void OnChanged()
		{
			if(Changed != null)
				Changed(this, EventArgs.Empty);
		}

		/// <summary>
		/// Gets the operator manager context.
		/// </summary>
		public TContext Context
		{
			get { return context; }
		}

		/// <summary>
		/// Fired after a step has been added, applied or unapplied.
		/// </summary>
		public event EventHandler Changed;

		/// <summary>
		/// Fires the <see cref="PropertyChanged"/> event for property <paramref name="propertyName"/>.
		/// </summary>
		/// <param name="propertyName"></param>
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if(PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#region INotifyPropertyChanged Members

		/// <inheritdoc/>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

	}

}
