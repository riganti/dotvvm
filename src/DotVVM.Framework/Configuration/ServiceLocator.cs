using System;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Configuration
{
    public class ServiceLocator
    {
        private IServiceCollection serviceCollection;
        private IServiceProvider serviceProvider;

        public ServiceLocator(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            this.serviceProvider = serviceProvider;
        }

        public ServiceLocator(IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            this.serviceCollection = serviceCollection;
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

        public T GetService<T>() 
            => (T)GetServiceProvider().GetService(typeof(T));

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

        [Obsolete]
        private void RegisterService<T>(Func<T> factory, ServiceLifetime lifetime)
        {
            if (serviceCollection == null)
            {
                throw new InvalidOperationException("Could not register service to ServiceLocator that has already built IServiceProvider.");
            }

            serviceCollection.Add(new ServiceDescriptor(typeof(T), p => factory(), lifetime));
        }
    }
}