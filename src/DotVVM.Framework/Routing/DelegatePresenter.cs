using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Routing
{
    public class DelegatePresenter : IDotvvmPresenter
    {
        private readonly Action<IDotvvmRequestContext> action;

        public DelegatePresenter(Action<IDotvvmRequestContext> action)
        {
            this.action = action;
        }

        public Task ProcessRequest(IDotvvmRequestContext context)
        {
            return Task.Run(() => action(context));
        }
    }
}
