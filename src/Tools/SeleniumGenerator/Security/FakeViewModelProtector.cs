using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class FakeViewModelProtector : IViewModelProtector
    {
        public string Protect(string serializedData, IDotvvmRequestContext context) => serializedData;

        public byte[] Protect(byte[] plaintextData, params string[] purposes) => plaintextData;

        public string Unprotect(string protectedData, IDotvvmRequestContext context) => protectedData;

        public byte[] Unprotect(byte[] protectedData, params string[] purposes) => protectedData;
    }
}
