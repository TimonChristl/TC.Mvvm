using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm.Injector
{

    internal abstract class BaseBinding
    {

        private Injector injector;

        public BaseBinding(Injector injector)
        {
            this.injector = injector;
        }

        public abstract object GetInstance();

        public abstract bool IsNewInstance { get; }

        protected Injector Injector
        {
            get { return injector; }
        }

    }

    internal abstract class BaseBinding<T> : BaseBinding
    {

        public BaseBinding(Injector injector)
            : base(injector)
        {
        }

        public override object GetInstance()
        {
            return GetInstanceCore();
        }

        public abstract T GetInstanceCore();

    }

    internal class InstanceBinding<T> : BaseBinding<T>
    {

        private T instance;

        public InstanceBinding(Injector injector, T instance)
            : base(injector)
        {
            this.instance = instance;
        }

        public override T GetInstanceCore()
        {
            return instance;
        }

        public override bool IsNewInstance
        {
            get { return false; }
        }

    }

    internal class FactoryBinding<T> : BaseBinding<T>
    {

        private Func<T> factory;
        private bool singleton;

        public FactoryBinding(Injector injector, Func<T> factory, bool singleton)
            : base(injector)
        {
            if(factory == null)
                throw new ArgumentNullException(nameof(factory));

            this.factory = factory;
            this.singleton = singleton;
        }

        public override T GetInstanceCore()
        {
            if(singleton)
            {
                object instance;
                if(!Injector.Singletons.TryGetValue(typeof(T), out instance))
                {
                    instance = factory();
                    Injector.Singletons.Add(typeof(T), instance);
                }

                return (T)instance;
            }
            else
            {
                return factory();
            }
        }

        public override bool IsNewInstance
        {
            get { return !singleton; }
        }

    }

}
