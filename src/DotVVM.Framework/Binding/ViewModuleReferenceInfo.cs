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

        /// <summary> Whether control id should be used instead of ViewId to identify the modules. </summary>
        public bool IsMarkupControl { get; }

        public ViewModuleReferenceInfo(string spaceId, string[] referencedModules, bool isMarkupControl)
        {
            this.SpaceId = spaceId;
            this.ReferencedModules = referencedModules;
            this.Resource = new ViewModuleImportResource(referencedModules);
            this.IsMarkupControl = isMarkupControl;
        }

        public ViewModuleImportResource Resource { get; }
    }
}
