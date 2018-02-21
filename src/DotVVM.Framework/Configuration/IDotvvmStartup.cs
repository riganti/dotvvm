using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Configuration
{
    public interface IDotvvmStartup
    {
        void Configure(DotvvmConfiguration config, string applicationPath);
        void ConfigureServices(IDotvvmServiceCollection serviceCollection);
    }
}
