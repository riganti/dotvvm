using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Security {
    public interface IViewModelProtector {

        string Protect(string serializedData, DotvvmRequestContext context);

        string Unprotect(string protectedData, DotvvmRequestContext context);

    }
}
