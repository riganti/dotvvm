using System;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Runtime.ControlTree.DesignTime
{
    public class DesignTimeBinding : IAbstractBinding
    {
        public DothtmlBindingNode BindingNode { get; }

        public Type BindingType { get; }

        public string Value => BindingNode.Value;

        public IDataContextStack DataContextTypeStack { get; }

        public ITypeDescriptor ResultType
        {
            get { throw new NotImplementedException(); }
        }


        public DesignTimeBinding(DothtmlBindingNode node, Type bindingType, IDataContextStack dataContextTypeStack)
        {
            BindingNode = node;
            BindingType = bindingType;
            DataContextTypeStack = dataContextTypeStack;
        }
    }
}
