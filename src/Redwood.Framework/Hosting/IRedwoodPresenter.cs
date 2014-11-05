using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redwood.Framework.Hosting
{
    public interface IRedwoodPresenter
    {
        /// <summary>
        /// Processes the request.
        /// </summary>
        Task ProcessRequest(RedwoodRequestContext context);
    }
}