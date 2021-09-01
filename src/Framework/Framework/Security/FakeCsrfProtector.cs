using System;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Security
{
    internal class FakeCsrfProtector : ICsrfProtector
    {
        public string GenerateToken(IDotvvmRequestContext context)
        {
            return "Not a CSRF token.";
        }

        public void VerifyToken(IDotvvmRequestContext context, string token)
        {
            if (token != "Not a CSRF token.")
                throw new Exception();
        }
    }
}
