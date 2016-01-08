using System;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Runtime.Compilation.AbstractControlTree
{
    public interface IAbstractBinding
    {

        DothtmlBindingNode BindingNode { get; }

        Type BindingType { get; }

        string Value { get; }
        
        IDataContextStack DataContextTypeStack { get; }
        
        ITypeDescriptor ResultType { get; }

    }
}