using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Configuration
{
    public class ServiceLocator
    {
        private Func<IServiceCollection, IServiceProvider> serviceProviderFactoryMethod;
        private IServiceCollection serviceCollection;
        private IServiceProvider serviceProvider;

        public ServiceLocator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ServiceLocator(IServiceCollection serviceCollection, Func<IServiceCollection, IServiceProvider> serviceProviderFactoryMethod = null)
        {
            this.serviceProviderFactoryMethod = serviceProviderFactoryMethod;
            this.serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }

        public IServiceProvider GetServiceProvider()
        {
            if (serviceProvider == null)
            {
                serviceProvider = (serviceProviderFactoryMethod ?? BuildServiceProvider).Invoke(serviceCollection);
                serviceCollection = null;
            }
            return serviceProvider;
        }

        private IServiceProvider BuildServiceProvider()
        {
            return BuildServiceProvider(serviceCollection);
        }

        public T GetService<T>()
            => GetServiceProvider().GetService<T>();

        /// <summary>
        /// Workaround for breaking change introduced in https://github.com/aspnet/DependencyInjection/pull/616
        /// In Microsoft.Extensions.DependencyInjection 2.0 signature of <see cref="ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(IServiceCollection)"/> has been changed slightly causing <see cref="MissingMethodException"/>.
        /// Lets bind it dynamically.
        /// Source: https://github.com/peachpiecompiler/peachpie/blob/eb9213f174fa909459ad0137ff996876eac2ac4c
        /// </summary>
        private static IServiceProvider BuildServiceProvider(IServiceCollection services)
        {
            // Template: return ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(services);

            var t = typeof(ServiceCollectionContainerBuilderExtensions).GetTypeInfo();
            var BuildServiceProviderMethod = t.GetMethod(nameof(BuildServiceProvider), new Type[] { typeof(IServiceCollection) });
            Debug.Assert(BuildServiceProviderMethod != null);

            return (IServiceProvider)BuildServiceProviderMethod.Invoke(null, new[] { services });
        }
    }
}
