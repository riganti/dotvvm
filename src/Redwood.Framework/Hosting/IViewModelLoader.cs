using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Hosting
{
    public interface IViewModelLoader
    {

        object InitializeViewModel(RedwoodRequestContext context, RedwoodView view);

    }
}