using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.ResourceManagement
{
    public class ClientModuleResourceRepository : CachingResourceRepository
    {
        private readonly Lazy<IClientModuleCompiler> clientModuleCompiler;

        public ClientModuleResourceRepository(Lazy<IClientModuleCompiler> clientModuleCompiler)
        {
            this.clientModuleCompiler = clientModuleCompiler;
        }

        protected override IResource FindResource(string name)
        {
            var code = clientModuleCompiler.Value.GetClientModuleResourceScript(name);
            return new InlineScriptResource(code, defer: true) { Dependencies = new[] { ResourceConstants.DotvvmResourceName } };
        }
    }
}
