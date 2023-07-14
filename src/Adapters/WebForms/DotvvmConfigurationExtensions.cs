using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Adapters.WebForms.Controls;
using DotVVM.Framework.Configuration;

namespace DotVVM.Adapters.WebForms
{
    public static class DotvvmConfigurationExtensions
    {

        public static void AddWebFormsAdapters(this DotvvmConfiguration config)
        {
            config.Markup.AddCodeControls("webforms", typeof(HybridRouteLink));
            config.Markup.Assemblies.Add(typeof(DotvvmConfigurationExtensions).Assembly.FullName);
        }
    }
}
