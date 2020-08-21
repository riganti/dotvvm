#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using System.Globalization;

namespace DotVVM.Framework.ResourceManagement.ClientGlobalize
{
    public class JQueryGlobalizeResourceRepository : CachingResourceRepository
    {
        protected override IResource FindResource(string name) =>
            new ScriptResource(defer: true, location: new JQueryGlobalizeResourceLocation(new CultureInfo(name)))
            {
                Dependencies = new[] { ResourceConstants.GlobalizeResourceName },
            };
    }
}
