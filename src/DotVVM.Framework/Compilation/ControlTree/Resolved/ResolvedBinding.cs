using System;
using System.Diagnostics;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    [DebuggerDisplay("{Type.Name}: {Value}")]
    public class ResolvedBinding : ResolvedTreeNode, IAbstractBinding
    {
        public DothtmlBindingNode BindingNode => (DothtmlBindingNode)DothtmlNode;

        public Type BindingType { get; set; }

        public string Value { get; set; }

        public Expression Expression { get; set; }

        public DataContextStack DataContextTypeStack { get; set; }

        public Exception ParsingError { get; set; }

        public ITypeDescriptor ResultType { get; set; }

        IDataContextStack IAbstractBinding.DataContextTypeStack => DataContextTypeStack;

        IAbstractTreeNode IAbstractTreeNode.Parent => Parent;

        public DebugInfoExpression DebugInfo { get; set; }

        public Expression GetExpression()
        {
            if (ParsingError != null)
            {
                throw new DotvvmCompilationException($"The binding '{{{ BindingType.Name }: { Value }}}' is not valid!", ParsingError, DothtmlNode.Tokens);
            }
            return Expression;
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitBinding(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {          
        }
    }
}
