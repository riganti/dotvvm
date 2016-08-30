using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace DotVVM.Framework.Configuration
{
    public class ServiceLocator
    {
        private IServiceProvider serviceProvider;
        private IServiceCollection serviceCollection;

        public ServiceLocator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public ServiceLocator(IServiceCollection collection)
        {
            this.serviceCollection = collection;
        }

        public IServiceProvider GetServiceProvider()
        {
            if (serviceProvider == null)
            {
                serviceProvider = serviceCollection.BuildServiceProvider();
                serviceCollection = null;
            }
            return serviceProvider;
        }

        public T GetService<T>() => (T)GetServiceProvider().GetService(typeof(T));

        [Obsolete]
        private void RegisterService<T>(Func<T> factory, ServiceLifetime lifetime)
        {
            if (serviceCollection == null) throw new InvalidOperationException("Could not register service to ServiceLocator that has already built IServiceProvider.");
            serviceCollection.Add(new ServiceDescriptor(typeof(T), p => factory(), lifetime));
        }

        [Obsolete("You should not register service on ServiceLocator, use IServiceCollection instead")]
        public void RegisterTransient<T>(Func<T> factory)
        {
            RegisterService(factory, ServiceLifetime.Transient);
        }

        [Obsolete("You should not register service on ServiceLocator, use IServiceCollection instead")]
        public void RegisterSingleton<T>(Func<T> factory)
        {
            RegisterService(factory, ServiceLifetime.Singleton);
        }
    }
}
