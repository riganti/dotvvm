using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Adapters.WebForms.Controls;

// ReSharper disable once CheckNamespace
namespace DotVVM.Framework.Configuration
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
