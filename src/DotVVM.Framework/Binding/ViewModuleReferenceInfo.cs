using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Binding
{
    /// <summary>
    /// Reference to a javascript file.
    /// </summary>
    [HandleAsImmutableObjectInDotvvmPropertyAttribute]
    public sealed class ViewModuleReferenceInfo
    {
        public string[] ReferencedModules { get; }
        /// <summary>The modules are referenced under an Id to the dotvvm client-side runtime. The same ID must be used in the invocation from the _js literal.</summary>
        public string SpaceId { get; }

        public ViewModuleReferenceInfo(string spaceId, string[] referencedModules)
        {
            this.SpaceId = spaceId;
            this.ReferencedModules = referencedModules;
            this.Resource = new ViewModuleImportResource(referencedModules);
        }

        public ViewModuleImportResource Resource { get; }
    }
}
