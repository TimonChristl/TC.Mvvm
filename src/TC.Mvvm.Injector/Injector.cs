﻿using System;
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
        /// </summary>
        /// <param name="contractType"></param>
        /// <returns></returns>
        object Get(Type contractType);

        /// <summary>
        /// Gets an instance for the contract type <typeparamref name="TContract"/>.
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <returns></returns>
        TContract Get<TContract>();

        /// <summary>
        /// Resolves injected properties for the instance <paramref name="instance"/>. This method
        /// is useful for cases when the instance is not obtained via injection, but still needs some
        /// properties injected.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        TInstance Resolve<TInstance>(TInstance instance);

    }

    /// <summary>
    /// An injector request for an instance of contract type (<see cref="ContractType"/>. The request can either be for
    /// a top-level instance, or for an instance for a property attributed with <see cref="InjectAttribute"/> while resolving another object.
    /// </summary>
    public class InjectorRequest
    {

        private Type contractType;
        private object enclosingObject;
        private InjectAttribute attribute;

        internal InjectorRequest(Type contractType, object enclosingObject, InjectAttribute attribute)
        {
            this.contractType = contractType;
            this.enclosingObject = enclosingObject;
            this.attribute = attribute;
        }

        /// <summary>
        /// The contract type for the request.
        /// </summary>
        public Type ContractType
        {
            get { return contractType; }
        }

        /// <summary>
        /// The object that contains the property for which the instance is requested.
        /// If this property is <c>null</c>, then the requeust is for a top-level instance and not for a property, and <see cref="Attribute"/> is also <c>null</c>.
        /// </summary>
        public object EnclosingObject
        {
            get { return enclosingObject; }
        }

        /// <summary>
        /// The <see cref="InjectAttribute"/> for the property for which the instance is requested.
        /// If this property is <c>null</c>, then the request is for a top-level instance and not for a property, and <see cref="EnclosingObject"/> is also <c>null</c>.
        /// </summary>
        public InjectAttribute Attribute
        {
            get { return attribute; }
        }

    }

    /// <summary>
    /// A version of <see cref="InjectorRequest"/> that provides strongly-typed access to the <see cref="EnclosingObject"/> property.
    /// </summary>
    /// <typeparam name="TEnclosingObject"></typeparam>
    public class InjectorRequest<TEnclosingObject> : InjectorRequest
    {

        internal InjectorRequest(Type contractType, TEnclosingObject enclosingObject, InjectAttribute attribute)
            :base(contractType, enclosingObject, attribute)
        {
        }

        /// <summary>
        /// The strongly-typed object that contains the property for which the instance is requested.
        /// If this property is <c>null</c>, then the requeust is for a top-level instance and not for a property, and <see cref="Attribute"/> is also <c>null</c>.
        /// </summary>
        public new TEnclosingObject EnclosingObject
        {
            get { return (TEnclosingObject)base.EnclosingObject; }
        }

    }

    /// <summary>
    /// A simple dependency injector.
    /// </summary>
    public class Injector : IInjector
    {

        private class ConditionAndBinding
        {
            public Func<InjectorRequest, bool> Condition;
            public BaseBinding Binding;
        }

        private class PropertyInfoAndAttribute
        {
            public PropertyInfo PropertyInfo;
            public InjectAttribute Attribute;
        }

        private Dictionary<Type, List<ConditionAndBinding>> bindings = new Dictionary<Type, List<ConditionAndBinding>>();
        private Dictionary<Type, object> singletons = new Dictionary<Type, object>();
        private Dictionary<Type, PropertyInfoAndAttribute[]> injectedPropertiesCache = new Dictionary<Type, PropertyInfoAndAttribute[]>();

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

        private object GetCore(InjectorRequest request)
        {
            List<ConditionAndBinding> conditionsAndBindingsForType;
            if(!bindings.TryGetValue(request.ContractType, out conditionsAndBindingsForType))
                return null;

            BaseBinding binding = conditionsAndBindingsForType
                .Where(cab => cab.Condition == null || cab.Condition(request))
                .Select(cab => cab.Binding)
                .FirstOrDefault();

            if(binding == null)
                return null;

            object instance;
            var instanceNeedsResolve = binding.GetInstance(out instance);

            if(instance == null)
                return null;

            if(instanceNeedsResolve)
                Resolve(instance);

            return instance;
        }

        #region IInjector Members

        /// <inheritdoc/>
        public object Get(Type contractType)
        {
            return GetCore(new InjectorRequest(contractType, null, null));
        }

        /// <inheritdoc/>
        public TContract Get<TContract>()
        {
            return (TContract)GetCore(new InjectorRequest(typeof(TContract), null, null));
        }

        /// <inheritdoc/>
        public TInstance Resolve<TInstance>(TInstance instance)
        {
            var actualInstanceType = instance.GetType();

            PropertyInfoAndAttribute[] injectedPropertiesForType;
            if(!injectedPropertiesCache.TryGetValue(actualInstanceType, out injectedPropertiesForType))
            {
                injectedPropertiesForType = instance
                    .GetType()
                    .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Select(pi => new PropertyInfoAndAttribute { PropertyInfo = pi, Attribute = pi.GetCustomAttribute<InjectAttribute>(), })
                    .Where(x => x.Attribute != null)
                    .ToArray();

                injectedPropertiesCache.Add(actualInstanceType, injectedPropertiesForType);
            }

            foreach(var propertyInfoAndInjectAttribute in injectedPropertiesForType)
            {
                var propertyContractType = propertyInfoAndInjectAttribute.PropertyInfo.PropertyType;
                var propertyValue = GetCore(new InjectorRequest(propertyContractType, instance, propertyInfoAndInjectAttribute.Attribute));
                propertyInfoAndInjectAttribute.PropertyInfo.SetValue(instance, propertyValue);
            }

            return instance;
        }

        #endregion

        #region IServiceProvider Members

        /// <inheritdoc/>
        public object GetService(Type serviceType)
        {
            return GetCore(new InjectorRequest(serviceType, null, null));
        }

        #endregion

        internal void AddBinding(Type contractType, Func<InjectorRequest, bool> condition, BaseBinding binding)
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
