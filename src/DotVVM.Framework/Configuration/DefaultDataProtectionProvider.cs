using Microsoft.Owin.Security.DataProtection;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Configuration
{
    internal class DefaultDataProtectionProvider : IDataProtectionProvider
    {
        private IAppBuilder appBuilder;

        public DefaultDataProtectionProvider(IAppBuilder appBuilder)
        {
            this.appBuilder = appBuilder;
        }

        public IDataProtector Create(params string[] purposes)
        {
            return this.appBuilder.CreateDataProtector(new string[] { });
        }
    }
}
