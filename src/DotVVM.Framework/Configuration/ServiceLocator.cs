using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Configuration
{
    public class ServiceLocator
    {
        private IServiceCollection serviceCollection;
        private IServiceProvider serviceProvider;

        public ServiceLocator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ServiceLocator(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }

        public IServiceProvider GetServiceProvider()
        {
            if (serviceProvider == null)
            {
                serviceProvider = BuildServiceProvider();
                serviceCollection = null;
            }
            return serviceProvider;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IServiceProvider BuildServiceProvider()
        {
            return serviceCollection.BuildServiceProvider();
        }

        public T GetService<T>() 
            => GetServiceProvider().GetService<T>();

        [Obsolete("You should not register service on ServiceLocator, use IServiceCollection instead", true)]
        public void RegisterTransient<T>(Func<T> factory)
        {
            RegisterService(factory, ServiceLifetime.Transient);
        }

        [Obsolete("You should not register service on ServiceLocator, use IServiceCollection instead", true)]
        public void RegisterSingleton<T>(Func<T> factory)
        {
            RegisterService(factory, ServiceLifetime.Singleton);
        }

        [Obsolete("You should not register service on ServiceLocator, use IServiceCollection instead", true)]
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