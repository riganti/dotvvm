#nullable enable

using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractTreeRoot : IAbstractContentNode
    {
        Dictionary<string, List<IAbstractDirective>> Directives { get; }
        string? FileName { get; set; }
    }
}
