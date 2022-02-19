using System;
using System.Collections.Generic;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Framework.Controls.DynamicData
{
    // Many thanks to https://gist.github.com/khellang/c9d39444f713eab04c26dc09d5687196

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Decorate<TService>(this IServiceCollection services, Func<TService, IServiceProvider, TService> decorator)
            where TService: notnull
        {
            var descriptors = services.GetDescriptors<TService>();

            foreach (var descriptor in descriptors)
            {
                services.Replace(descriptor.Decorate(decorator));
            }

            return services;
        }

        public static IServiceCollection Decorate<TService>(this IServiceCollection services, Func<TService, TService> decorator)
            where TService: notnull
        {
            var descriptors = services.GetDescriptors<TService>();

            foreach (var descriptor in descriptors)
            {
                services.Replace(descriptor.Decorate(decorator));
            }

            return services;
        }

        private static List<ServiceDescriptor> GetDescriptors<TService>(this IServiceCollection services)
        {
            var descriptors = new List<ServiceDescriptor>();

            foreach (var service in services)
            {
                if (service.ServiceType == typeof(TService))
                {
                    descriptors.Add(service);
                }
            }

            if (descriptors.Count == 0)
            {
                throw new InvalidOperationException($"Could not find any registered services for type '{typeof(TService).FullName}'.");
            }

            return descriptors;
        }

        private static ServiceDescriptor Decorate<TService>(this ServiceDescriptor descriptor, Func<TService, IServiceProvider, TService> decorator)
            where TService : notnull
        {
            return descriptor.WithFactory(provider => decorator((TService)descriptor.GetInstance(provider).NotNull($"Service {descriptor.ServiceType} could not be found."), provider));
        }

        private static ServiceDescriptor Decorate<TService>(this ServiceDescriptor descriptor, Func<TService, TService> decorator)
            where TService : notnull
        {
            return descriptor.WithFactory(provider => decorator((TService)descriptor.GetInstance(provider).NotNull($"Service {descriptor.ServiceType} could not be found.")));
        }

        private static ServiceDescriptor WithFactory(this ServiceDescriptor descriptor, Func<IServiceProvider, object> factory)
        {
            return ServiceDescriptor.Describe(descriptor.ServiceType, factory, descriptor.Lifetime);
        }

        private static object GetInstance(this ServiceDescriptor descriptor, IServiceProvider provider)
        {
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationType != null)
            {
                return provider.GetServiceOrCreateInstance(descriptor.ImplementationType);
            }

            return descriptor.ImplementationFactory(provider);
        }

        private static object GetServiceOrCreateInstance(this IServiceProvider provider, Type type)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance(provider, type);
        }
    }
}
