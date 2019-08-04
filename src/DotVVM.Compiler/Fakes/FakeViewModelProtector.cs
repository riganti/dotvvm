using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;

namespace DotVVM.Compiler.Fakes
{
    public class FakeViewModelProtector :IViewModelProtector
    {
        public string Protect(string serializedData, IDotvvmRequestContext context)
        {
            return "";
        }

        public byte[] Protect(byte[] plaintextData, params string[] purposes)
        {
            return new byte[0];
        }

        public string Unprotect(string protectedData, IDotvvmRequestContext context)
        {
            return "";
        }

        public byte[] Unprotect(byte[] protectedData, params string[] purposes)
        {
            return new byte[0];
        }
    }
}
