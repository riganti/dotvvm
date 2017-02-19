using System;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DefaultViewModelLoader : IViewModelLoader
    {
        protected ObjectFactory CreateObjectFactory(Type viewModelType)
        {
            return ActivatorUtilities.CreateFactory(viewModelType, Type.EmptyTypes);
        }

        protected static ConcurrentDictionary<Type, ObjectFactory> factoryCache = new ConcurrentDictionary<Type, ObjectFactory>();

        /// <summary>
        /// Initializes the view model for the specified view.
        /// </summary>
        public object InitializeViewModel(IDotvvmRequestContext context, DotvvmView view)
        {
            return CreateViewModelInstance(view.ViewModelType, context);
        }

        /// <summary>
        /// Creates the new instance of a viewmodel of specified type. 
        /// If you are using IoC/DI container, this is the method you want to override.
        /// </summary>
        protected virtual object CreateViewModelInstance(Type viewModelType, IDotvvmRequestContext context)
        {
            return factoryCache.GetOrAdd(viewModelType, CreateObjectFactory).Invoke(context.Services, new object[0]);
        }

        /// <summary>
        /// Disposes the viewmodel instance (provided it is <see cref="IDisposable"/>. 
        /// If you are using IoC/DI container with manual component release mechanism, this it the method you want to override.
        /// </summary>
        public virtual void DisposeViewModel(object instance)
        {
            (instance as IDisposable)?.Dispose();
        }
    }
}