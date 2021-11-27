
using System.Collections.Generic;
using DotVVM.Framework.Compilation.ViewCompiler;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractTreeRoot : IAbstractControl
    {
        Dictionary<string, List<IAbstractDirective>> Directives { get; }
        string? FileName { get; set; }
        IAbstractControlBuilderDescriptor? MasterPage { get; }
    }
}
