using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm.Injector
{

    /// <summary>
    /// Interface for the dependency injector
    /// </summary>
    public interface IInjector : IServiceProvider
    {

        /// <summary>
        /// Gets an instance for the contract type <paramref name="contractType"/>.
        /// <paramref name="enclosingInstance"/> is passed to the context for evaluating binding conditions.
        /// </summary>
        /// <param name="contractType"></param>
        /// <param name="enclosingInstance"></param>
        /// <returns></returns>
        object Get(Type contractType, object enclosingInstance = null);

        /// <summary>
        /// Gets an instance for the contract type <typeparamref name="TContract"/>.
        /// <paramref name="enclosingInstance"/> is passed to the context for evaluating binding conditions.
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <param name="enclosingInstance"></param>
        /// <returns></returns>
        TContract Get<TContract>(object enclosingInstance = null);

    }

    /// <summary>
    /// A simple dependency injector.
    /// </summary>
    public class Injector : IInjector
    {

        private class ConditionAndBinding
        {
            public Func<BindingConditionContext, bool> Condition;
            public BaseBinding Binding;
        }

        private Dictionary<Type, List<ConditionAndBinding>> bindings = new Dictionary<Type, List<ConditionAndBinding>>();
        private Dictionary<Type, object> singletons = new Dictionary<Type, object>();

        /// <summary>
        /// Creates an instance of the fluent API helper for binding contract types to instances.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FluentBinder<T> Bind<T>()
        {
            if(!typeof(T).IsInterface && !typeof(T).IsClass)
                throw new InvalidOperationException("Type argument must be an interface or class type");

            return new FluentBinder<T>(this);
        }

        #region IInjector Members

        /// <inheritdoc/>
        public object Get(Type contractType, object enclosingInstance = null)
        {
            List<ConditionAndBinding> conditionsAndBindingsForType;
            if(!bindings.TryGetValue(contractType, out conditionsAndBindingsForType))
                return null;

            BindingConditionContext context = new BindingConditionContext(contractType, enclosingInstance);

            BaseBinding binding = conditionsAndBindingsForType
                .Where(cab => cab.Condition == null || cab.Condition(context))
                .Select(cab => cab.Binding)
                .FirstOrDefault();

            if(binding == null)
                return null;

            var instance = binding.GetInstance();
            if(instance == null)
                return null;

            if(binding.IsNewInstance)
                Resolve(instance);

            return instance;
        }

        /// <inheritdoc/>
        public TContract Get<TContract>(object enclosingInstance = null)
        {
            return (TContract)Get(typeof(TContract), enclosingInstance);
        }

        private Dictionary<Type, Tuple<PropertyInfo, InjectAttribute>[]> injectedPropertiesCache = new Dictionary<Type, Tuple<PropertyInfo, InjectAttribute>[]>();

        /// <summary>
        /// Resolves injected properties for the instance <paramref name="instance"/>.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public TInstance Resolve<TInstance>(TInstance instance)
        {
            var actualInstanceType = instance.GetType();

            Tuple<PropertyInfo, InjectAttribute>[] injectedPropertiesForType;
            if(!injectedPropertiesCache.TryGetValue(actualInstanceType, out injectedPropertiesForType))
            {
                injectedPropertiesForType = instance
                    .GetType()
                    .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Select(pi => new Tuple<PropertyInfo, InjectAttribute>(pi, pi.GetCustomAttribute<InjectAttribute>()))
                    .Where(x => x.Item2 != null)
                    .ToArray();

                injectedPropertiesCache.Add(actualInstanceType, injectedPropertiesForType);
            }

            foreach(var propertyInfoAndInjectAttribute in injectedPropertiesForType)
            {
                var propertyContractType = propertyInfoAndInjectAttribute.Item1.PropertyType;
                var propertyValue = Get(propertyContractType, instance);
                propertyInfoAndInjectAttribute.Item1.SetValue(instance, propertyValue);
            }

            return instance;
        }

        #endregion

        #region IServiceProvider Members

        /// <inheritdoc/>
        public object GetService(Type serviceType)
        {
            return Get(serviceType, null);
        }

        #endregion

        internal void AddBinding(Type contractType, Func<BindingConditionContext, bool> condition, BaseBinding binding)
        {
            List<ConditionAndBinding> conditionsAndBindingsForContractType;
            if(!bindings.TryGetValue(contractType, out conditionsAndBindingsForContractType))
            {
                conditionsAndBindingsForContractType = new List<ConditionAndBinding>();
                bindings.Add(contractType, conditionsAndBindingsForContractType);
            }

            if(conditionsAndBindingsForContractType.Any(cab => cab.Condition == condition))
                throw new InvalidOperationException("A binding for this contractType and condition has already been added");

            conditionsAndBindingsForContractType.Add(new ConditionAndBinding
            {
                Condition = condition,
                Binding = binding,
            });
        }

        internal Dictionary<Type, object> Singletons
        {
            get { return singletons; }
        }

    }

}
