using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    /// <summary> Represents the @js directive - import ES module on the client side </summary>
    public class ResolvedViewModuleDirective : ResolvedDirective, IAbstractViewModuleDirective
    {
        /// <summary>Original path specified by the module</summary>
        public string ImportedModule { get; }

        /// <summary>The imported resource that will be referenced at runtime</summary>
        public string ImportedResourceName { get; }
        public ResolvedViewModuleDirective(string importedModule, string importedResourceName)
        {
            ImportedResourceName = importedResourceName;
            ImportedModule = importedModule;
        }
    }
}
