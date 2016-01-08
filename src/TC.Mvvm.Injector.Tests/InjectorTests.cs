using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TC.Mvvm.Injector.Tests
{

    interface IPropertyContract
    {
    }

    interface IContract
    {
        void Method();

        [Inject]
        IPropertyContract Property { get; set; }

    }

    class ContractImplementation : IContract
    {
        public void Method()
        {
        }

        public IPropertyContract Property { get; set; }

    }

    class PropertyContractImplementation1 : IPropertyContract
    {
    }

    class PropertyContractImplementation2 : IPropertyContract
    {
    }

    [TestClass]
    public class InjectorTests
    {

        [TestMethod]
        public void Injector_BindToInstance_Works()
        {
            var contractImplementation = new ContractImplementation();

            var injector = new Injector();
            injector.Bind<IContract>().To(contractImplementation);

            Assert.AreEqual(injector.Get<IContract>(), contractImplementation);
        }

        [TestMethod]
        public void Injector_BindToFactory_Works()
        {
            var injector = new Injector();
            injector.Bind<IContract>().To(() => new ContractImplementation());

            Assert.IsInstanceOfType(injector.Get<IContract>(), typeof(ContractImplementation));
            Assert.AreNotEqual(injector.Get<IContract>(), injector.Get<IContract>());
        }

        [TestMethod]
        public void Injector_BindToImplementationType_Works()
        {
            var injector = new Injector();
            injector.Bind<IContract>().To<ContractImplementation>();

            Assert.IsInstanceOfType(injector.Get<IContract>(), typeof(ContractImplementation));
            Assert.AreNotEqual(injector.Get<IContract>(), injector.Get<IContract>());
        }

        [TestMethod]
        public void Injector_BindToFactory_Singleton_Works()
        {
            var injector = new Injector();
            injector.Bind<IContract>().ToSingleton(() => new ContractImplementation());

            Assert.IsInstanceOfType(injector.Get<IContract>(), typeof(ContractImplementation));
            Assert.AreEqual(injector.Get<IContract>(), injector.Get<IContract>());
        }

        [TestMethod]
        public void Injector_BindToImplementationType_Singleton_Works()
        {
            var injector = new Injector();
            injector.Bind<IContract>().ToSingleton<ContractImplementation>();

            Assert.IsInstanceOfType(injector.Get<IContract>(), typeof(ContractImplementation));
            Assert.AreEqual(injector.Get<IContract>(), injector.Get<IContract>());
        }

        [TestMethod]
        public void Injector_BindWithIf_Works()
        {
            var injector = new Injector();

            injector.Bind<IContract>().To<ContractImplementation>();

            var instance1 = injector.Get<IContract>();
            var instance2 = injector.Get<IContract>();

            injector.Bind<IPropertyContract>().If((context) => context.EnclosingInstance == instance1).To<PropertyContractImplementation1>();
            injector.Bind<IPropertyContract>().If((context) => context.EnclosingInstance == instance2).To<PropertyContractImplementation2>();

            Assert.IsInstanceOfType(injector.Get<IContract>(), typeof(ContractImplementation));
            Assert.AreEqual(injector.Get<IContract>(), injector.Get<IContract>());
        }

    }

}
