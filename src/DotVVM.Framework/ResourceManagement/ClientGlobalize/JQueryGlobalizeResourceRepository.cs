using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement.ClientGlobalize
{
    public class JQueryGlobalizeResourceRepository : IDotvvmResourceRepository
    {
        public ResourceBase FindResource(string name)
        {
            return new ScriptResource()
            {
                Url = string.Format("~/{0}?{1}={2}", HostingConstants.GlobalizeCultureUrlPath, HostingConstants.GlobalizeCultureUrlIdParameter, name),
                Dependencies = new[] { ResourceConstants.GlobalizeResourceName },
                // TODO: cdn?
            };
        }
    }
}
