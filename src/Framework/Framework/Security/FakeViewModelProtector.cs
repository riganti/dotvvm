
using System;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Security
{
    internal class FakeViewModelProtector : IViewModelProtector
    {
        public byte[] Protect(byte[] serializedData, IDotvvmRequestContext context)
        {
            return [];
        }

        public byte[] Protect(byte[] plaintextData, params string[] purposes)
        {
            return [];
        }

        public byte[] Unprotect(byte[] protectedData, IDotvvmRequestContext context)
        {
            return [];
        }

        public byte[] Unprotect(byte[] protectedData, params string[] purposes)
        {
            return [];
        }
    }
}
