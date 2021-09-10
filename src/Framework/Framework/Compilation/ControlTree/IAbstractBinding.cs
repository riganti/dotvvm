using System;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractBinding : IAbstractTreeNode
    {
        Type BindingType { get; }

        string Value { get; }
        
        IDataContextStack DataContextTypeStack { get; }
        
        ITypeDescriptor? ResultType { get; }

    }
}
