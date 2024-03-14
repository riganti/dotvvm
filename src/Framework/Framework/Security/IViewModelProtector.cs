using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Security
{
    public interface IViewModelProtector
    {

        byte[] Protect(byte[] serializedData, IDotvvmRequestContext context);
        byte[] Protect(byte[] plaintextData, params string[] purposes);

        byte[] Unprotect(byte[] protectedData, IDotvvmRequestContext context);
        byte[] Unprotect(byte[] protectedData, params string[] purposes);

    }
}
