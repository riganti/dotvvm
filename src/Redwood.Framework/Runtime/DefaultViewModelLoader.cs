using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls;
using Redwood.Framework.Hosting;
using Redwood.Framework.Parser;

namespace Redwood.Framework.Runtime
{
    public class DefaultViewModelLoader : IViewModelLoader
    {
        /// <summary>
        /// Initializes the view model for the specified view.
        /// </summary>
        public object InitializeViewModel(RedwoodRequestContext context, RedwoodView view)
        {
            string viewModel;
            if (!view.Directives.TryGetValue(Constants.ViewModelDirectiveName, out viewModel))
            {
                throw new Exception("Couldn't find a viewmodel for the specified view!");       // TODO: exception handling
            }

            var viewModelType = Type.GetType(viewModel);
            if (viewModelType == null)
            {
                throw new Exception(string.Format("Couldn't create a class of type '{0}'!", viewModel));       // TODO: exception handling
            }
            return Activator.CreateInstance(viewModelType);
        }
    }
}