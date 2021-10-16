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
    public class DefaultDataProtectionProvider : IDataProtectionProvider
    {
        private IAppBuilder appBuilder;
        private IDataProtectionProvider defaultProvider;

        public DefaultDataProtectionProvider(IAppBuilder appBuilder)
        {
            this.appBuilder = appBuilder;

            defaultProvider = appBuilder.GetDataProtectionProvider();
        }

        public IDataProtector Create(params string[] purposes)
        {
            return defaultProvider?.Create(purposes) ?? appBuilder.CreateDataProtector(purposes);
        }
    }
}

