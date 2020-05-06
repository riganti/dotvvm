using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Diagnostics.StatusPage.Sample.Presenter
{
    public class NoPresenter : IDotvvmPresenter
    {
        public Task ProcessRequest(IDotvvmRequestContext context)
        {
            return Task.CompletedTask;
        }
    }
}
