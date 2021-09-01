using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Configuration
{
    public interface IDotvvmStartup
    {
        void Configure(DotvvmConfiguration config, string applicationPath);
    }
}
