using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Hosting
{
    public interface IRedwoodPresenter
    {

        IViewModelSerializer ViewModelSerializer { get; }

        /// <summary>
        /// Processes the request.
        /// </summary>
        Task ProcessRequest(RedwoodRequestContext context);
    }
}