#nullable enable
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IDataContextStack
    {
        ITypeDescriptor DataContextType { get; }

        IDataContextStack? Parent { get; }

        IReadOnlyList<NamespaceImport> NamespaceImports { get; }

        IReadOnlyList<BindingExtensionParameter> ExtensionParameters { get; }
        IEnumerable<(int dataContextLevel, BindingExtensionParameter parameter)> GetCurrentExtensionParameters();

    }
}
