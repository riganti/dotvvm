using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime
{
    public interface IViewModelLoader
    {

        object InitializeViewModel(DotvvmRequestContext context, DotvvmView view);

        void DisposeViewModel(object instance);

    }
}