using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime
{
    public class DefaultViewModelLoader : IViewModelLoader
    {
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
            return Activator.CreateInstance(viewModelType);
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