﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TC.Mvvm.Injector.Tests
{

    interface IPropertyContract
    {
    }

    interface IContract
    {
        void Method();

        IPropertyContract Property1 { get; set; }

        IPropertyContract Property2 { get; set; }
    }

    class ContractImplementation1 : IContract
    {
        public void Method()
        {
        }

        [Inject(Tag = "A")]
        public IPropertyContract Property1 { get; set; }

        [Inject(Tag = "B")]
        public IPropertyContract Property2 { get; set; }

        public bool Debug { get; set; }

    }

    class ContractImplementation2 : IContract
    {
        public void Method()
        {
        }

        [Inject(Tag = "A")]
        public IPropertyContract Property1 { get; set; }

        [Inject(Tag = "B")]
        public IPropertyContract Property2 { get; set; }

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
            var contractImplementation = new ContractImplementation1();

            var injector = new Injector();
            injector.Bind<IContract>().To(contractImplementation);

            Assert.AreEqual(injector.Get<IContract>(), contractImplementation);
        }

        [TestMethod]
        public void Injector_BindToFactory_Works()
        {
            var injector = new Injector();
            injector.Bind<IContract>().To(() => new ContractImplementation1());

            Assert.IsInstanceOfType(injector.Get<IContract>(), typeof(ContractImplementation1));
            Assert.AreNotEqual(injector.Get<IContract>(), injector.Get<IContract>());
        }

        [TestMethod]
        public void Injector_BindToImplementationType_Works()
        {
            var injector = new Injector();
            injector.Bind<IContract>().To<ContractImplementation1>();

            Assert.IsInstanceOfType(injector.Get<IContract>(), typeof(ContractImplementation1));
            Assert.AreNotEqual(injector.Get<IContract>(), injector.Get<IContract>());
        }

        [TestMethod]
        public void Injector_BindToFactory_Singleton_Works()
        {
            var injector = new Injector();
            injector.Bind<IContract>().ToSingleton(() => new ContractImplementation1());

            Assert.IsInstanceOfType(injector.Get<IContract>(), typeof(ContractImplementation1));
            Assert.AreEqual(injector.Get<IContract>(), injector.Get<IContract>());
        }

        [TestMethod]
        public void Injector_BindToImplementationType_Singleton_Works()
        {
            var injector = new Injector();
            injector.Bind<IContract>().ToSingleton<ContractImplementation1>();

            Assert.IsInstanceOfType(injector.Get<IContract>(), typeof(ContractImplementation1));
            Assert.AreEqual(injector.Get<IContract>(), injector.Get<IContract>());
        }

        [TestMethod]
        public void Injector_BindWithIf_Works_1()
        {
            var injector = new Injector();

            injector.Bind<IContract>().To<ContractImplementation1>();
            injector.Bind<IPropertyContract>().If((request) => request.Attribute.Tag == "A").To<PropertyContractImplementation1>();
            injector.Bind<IPropertyContract>().If((request) => request.Attribute.Tag == "B").To<PropertyContractImplementation2>();

            var instance = injector.Get<IContract>();

            Assert.IsInstanceOfType(instance.Property1, typeof(PropertyContractImplementation1));
            Assert.IsInstanceOfType(instance.Property2, typeof(PropertyContractImplementation2));
        }

        // ------------------------------------------------------------------------------------------------------------

        interface INetworkService
        {
            bool Debug { get; }
        }

        interface INetworkTracer
        {
            void Trace(string msg);
        }

        class NetworkService : INetworkService
        {
            public bool Debug { get; set; }

            [Inject]
            public INetworkTracer Tracer { get; set; }
        }

        class DebugNetworkTracer : INetworkTracer
        {
            public void Trace(string msg) { }
        }

        class NonDebugNetworkTracer : INetworkTracer
        {
            public void Trace(string msg) { }
        }

        [TestMethod]
        public void Injector_BindWithIf_Works_2()
        {
            var injector = new Injector();

            injector.Bind<INetworkTracer>().If<INetworkService>((request) => request.EnclosingObject.Debug).To<DebugNetworkTracer>();
            injector.Bind<INetworkTracer>().If<INetworkService>((request) => !request.EnclosingObject.Debug).To<NonDebugNetworkTracer>();

            var instance1 = new NetworkService { Debug = true, };
            injector.Resolve(instance1);

            var instance2 = new NetworkService { Debug = false, };
            injector.Resolve(instance2);

            Assert.IsInstanceOfType(instance1.Tracer, typeof(DebugNetworkTracer));
            Assert.IsInstanceOfType(instance2.Tracer, typeof(NonDebugNetworkTracer));
        }

    }

}
