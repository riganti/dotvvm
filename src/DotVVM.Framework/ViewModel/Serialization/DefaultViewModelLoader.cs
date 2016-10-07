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

		protected ConcurrentDictionary<Type, ObjectFactory> facotryCache = new ConcurrentDictionary<Type, ObjectFactory>();
		protected readonly IServiceProvider serviceProvider;

		public DefaultViewModelLoader(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

        /// <summary>
        /// Initializes the view model for the specified view.
        /// </summary>
        public object InitializeViewModel(IDotvvmRequestContext context, DotvvmView view)
        {
            return CreateViewModelInstance(view.ViewModelType);
        }

        /// <summary>
        /// Creates the new instance of a viewmodel of specified type. 
        /// If you are using IoC/DI container, this is the method you want to override.
        /// </summary>
        protected virtual object CreateViewModelInstance(Type viewModelType)
        {
			return facotryCache.GetOrAdd(viewModelType, CreateObjectFactory).Invoke(serviceProvider, new object[0]);
        }

        /// <summary>
        /// Disposes the viewmodel instance (provided it is <see cref="IDisposable"/>. 
        /// If you are using IoC/DI container with manual component release mechanism, this it the method you want to override.
        /// </summary>
        public virtual void DisposeViewModel(object instance)
        {
            if (instance is IDisposable)
            {
                ((IDisposable)instance).Dispose();
            }
        }
    }
}