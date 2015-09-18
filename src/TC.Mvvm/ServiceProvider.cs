#if DEBUG
#define ENABLE_SERVICES_TRACING
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

    /// <summary>
    /// Manages services by mapping service types (typically interfaces) to service instances.
    /// Services can be registered, which binds a service type to a service instance, can be unregistered, which
    /// unbinds a service instance from a service type, and can be retrieved, which returns the
    /// service instance for a given service type.
    /// </summary>
    /// <remarks>
    /// This class implements the standard .NET interface <see cref="IServiceProvider"/>.
    /// </remarks>
    public class ServiceProvider : IServiceProvider
    {

        private object lockObj = new object();
        private Dictionary<Type, object> services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers the supplied service instance for service type <typeparamref name="T"/>.
        /// If a service instance is already registered for the service type,
        /// <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <exception cref="ArgumentException">Service is not registered</exception>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public void Register<T>(T service)
        {
            lock(lockObj)
            {
                object existingService;
                if(!services.TryGetValue(typeof(T), out existingService))
                {
                    services.Add(typeof(T), service);

#if ENABLE_SERVICES_TRACING
                    Trace.WriteLine($"ServiceProvider: registered service {typeof(T).FullName}");
#endif
                }
                else
                    throw new ArgumentException("Service to be registered is already registered");
            }
        }

        /// <summary>
        /// Unregisters the supplied service instance for service type <typeparamref name="T"/>.
        /// If the service instance does not match the service registered for the service type,
        /// <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <exception cref="ArgumentException">Service is not registered</exception>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public void Unregister<T>(T service)
        {
            lock (lockObj)
            {
                object existingService;
                if(services.TryGetValue(typeof(T), out existingService) && object.ReferenceEquals(existingService, service))
                {
                    services.Remove(typeof(T));

#if ENABLE_SERVICES_TRACING
                    Trace.WriteLine($"ServiceProvider: unregistered service {typeof(T).FullName}");
#endif
                }
                else
                    throw new ArgumentException("Service to be unregistered is not registered");
            }
        }

        /// <summary>
        /// Retrieves the service instance for service type <typeparamref name="T"/>, or <c>null</c> if no
        /// such service is registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <remarks>
        /// The service type must be exactly the type supplied when the service was registered. Registering a service
        /// with a class type, then retrieving it by an interface type does not work, even if the class type implements that interface.
        /// </remarks>
        /// <returns></returns>
        public T Get<T>()
        {
            return (T)GetService(typeof(T));
        }

        /// <summary>
        /// Retrieves the service instance for service type <paramref name="serviceType"/>, or <c>null</c> if no
        /// such service is registered.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <remarks>
        /// The service type must be exactly the type supplied when the service was registered. Registering a service
        /// with a class type, then retrieving it by an interface type does not work, even if the class type implements that interface.
        /// </remarks>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            lock (lockObj)
            {
                object existingService;
                if(services.TryGetValue(serviceType, out existingService))
                    return existingService;
                else
                    return null;
            }
        }

        #region Singleton implementation

        private static ServiceProvider instance = null;

        private ServiceProvider()
        {
        }

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static ServiceProvider Instance
        {
            get
            {
                if(instance == null)
                    instance = new ServiceProvider();

                return instance;
            }
        }

        #endregion

    }

}
