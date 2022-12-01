using System;
using System.Collections.Immutable;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.ViewCompiler
{
    public interface IControlBuilder
    {
        ControlBuilderDescriptor Descriptor { get; }
        DotvvmControl BuildControl(IControlBuilderFactory controlBuilderFactory, IServiceProvider services);
    }

    public interface IAbstractControlBuilderDescriptor
    {
        ITypeDescriptor DataContextType { get; }
        ITypeDescriptor ControlType { get; }
        string? FileName { get; }
        IAbstractControlBuilderDescriptor? MasterPage { get; }
        ImmutableArray<(string name, string value)> Directives { get; }
    }
}
