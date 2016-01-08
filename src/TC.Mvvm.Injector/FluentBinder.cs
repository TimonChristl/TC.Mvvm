using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm.Injector
{

    /// <summary>
    /// Context for evaluating conditions when selecting bindings.
    /// </summary>
    public class BindingConditionContext
    {

        private Type contractType;
        private object enclosingInstance;

        internal BindingConditionContext(Type contractType, object enclosingInstance)
        {
            this.contractType = contractType;
            this.enclosingInstance = enclosingInstance;
        }

        /// <summary>
        /// The contract type for which a binding is about to be selected.
        /// </summary>
        public Type ContractType
        {
            get { return contractType; }
        }

        /// <summary>
        /// The enclosing instance for which a binding is about to be selected.
        /// </summary>
        public object EnclosingInstance
        {
            get { return enclosingInstance; }
        }

    }

    /// <summary>
    /// Helper for defining bindings from binding contract types to instances.
    /// </summary>
    /// <typeparam name="TContract"></typeparam>
    public class FluentBinder<TContract>
    {

        private Injector injector;
        private Func<BindingConditionContext, bool> condition = null;

        internal FluentBinder(Injector injector)
        {
            this.injector = injector;
        }

        /// <summary>
        /// Creates a binding of the contract type <typeparamref name="TContract"/> to the given instance <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public void To(TContract instance)
        {
            injector.AddBinding(typeof(TContract), condition, new InstanceBinding<TContract>(injector, instance));
        }

        /// <summary>
        /// Creates a binding of the contract type <typeparamref name="TContract"/> to instances created by <paramref name="factory"/>.
        /// Each time an instance is requested for this binding, a new instance will be created.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="singleton"></param>
        /// <returns></returns>
        public void To(Func<TContract> factory, bool singleton = false)
        {
            injector.AddBinding(typeof(TContract), condition, new FactoryBinding<TContract>(injector, factory, false));
        }

        /// <summary>
        /// Creates a binding of the contract type <typeparamref name="TContract"/> to a singleton instance created by <paramref name="factory"/>.
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public void ToSingleton(Func<TContract> factory)
        {
            injector.AddBinding(typeof(TContract), condition, new FactoryBinding<TContract>(injector, factory, true));
        }

        /// <summary>
        /// Creates a binding of the contract type <typeparamref name="TContract"/> to instances of the implementation type
        /// <typeparamref name="TImplementation"/> (which must derive from the contract type).
        /// </summary>
        /// <remarks>
        /// The implementation type must provide a parameter-less constructor.
        /// </remarks>
        /// <typeparam name="TImplementation"></typeparam>
        /// <returns></returns>
        public void To<TImplementation>()
            where TImplementation : TContract, new()
        {
            injector.AddBinding(typeof(TContract), condition, new FactoryBinding<TContract>(injector, () => new TImplementation(), false));
        }

        /// <summary>
        /// Creates a binding of the contract type <typeparamref name="TContract"/> to a singletone instance of the implementation type
        /// <typeparamref name="TImplementation"/> (which must derive from the contract type).
        /// </summary>
        /// <remarks>
        /// The implementation type must provide a parameter-less constructor.
        /// </remarks>
        /// <typeparam name="TImplementation"></typeparam>
        /// <returns></returns>
        public void ToSingleton<TImplementation>()
            where TImplementation : TContract, new()
        {
            injector.AddBinding(typeof(TContract), condition, new FactoryBinding<TContract>(injector, () => new TImplementation(), true));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public FluentBinder<TContract> If(Func<BindingConditionContext, bool> condition)
        {
            this.condition = condition;
            return this;
        }

    }

}
