using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Hosting
{
    public interface IDotvvmPresenter
    {

        IViewModelSerializer ViewModelSerializer { get; }

        /// <summary>
        /// Processes the request.
        /// </summary>
        Task ProcessRequest(DotvvmRequestContext context);
    }
}