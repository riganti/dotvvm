#nullable enable
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

        string Protect(string serializedData, IDotvvmRequestContext context);
        byte[] Protect(byte[] plaintextData, params string[] purposes);

        string Unprotect(string protectedData, IDotvvmRequestContext context);
        byte[] Unprotect(byte[] protectedData, params string[] purposes);

    }
}
