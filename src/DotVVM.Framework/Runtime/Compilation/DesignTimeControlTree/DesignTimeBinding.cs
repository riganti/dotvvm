using System;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.DesignTimeControlTree
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
