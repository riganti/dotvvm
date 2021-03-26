using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractViewModuleDirective : IAbstractDirective
    {
        /// <summary>Original path specified by the module</summary>
        string ImportedModule { get; }

        /// <summary>The imported resource that will be referenced at runtime</summary>
        string ImportedResourceName { get; }
    }
}
