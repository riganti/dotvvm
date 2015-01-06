using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls.Infrastructure;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime.Filters;

namespace Redwood.Framework.Runtime
{
    public interface IViewModelSerializer
    {

        string SerializeViewModel(RedwoodRequestContext context, RedwoodView view);

        string SerializeRedirectAction(RedwoodRequestContext context, string url);

        void PopulateViewModel(RedwoodRequestContext context, RedwoodView view, string serializedPostData);

        void ResolveCommand(RedwoodRequestContext context, RedwoodView view, string serializedPostData, out ActionInfo actionInfo);
    }
}