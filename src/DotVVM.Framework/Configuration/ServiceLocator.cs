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
    }
}