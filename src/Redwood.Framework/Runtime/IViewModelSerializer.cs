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
        void BuildViewModel(RedwoodRequestContext context, RedwoodView view);

        string SerializeViewModel(RedwoodRequestContext context);
        
        string SerializeModelState(RedwoodRequestContext context);

        void PopulateViewModel(RedwoodRequestContext context, RedwoodView view, string serializedPostData);

        void ResolveCommand(RedwoodRequestContext context, RedwoodView view, string serializedPostData, out ActionInfo actionInfo);

        void AddPostBackUpdatedControls(RedwoodRequestContext context);
    }
}