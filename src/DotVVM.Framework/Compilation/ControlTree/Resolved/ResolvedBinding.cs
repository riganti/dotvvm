using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    [DebuggerDisplay("{BindingType.Name}: {Value}")]
    public class ResolvedBinding : ResolvedTreeNode, IAbstractBinding
    {
        public IBinding Binding {get;}

        public BindingCompilationService BindingService { get; }

        public DothtmlBindingNode BindingNode => (DothtmlBindingNode)DothtmlNode;

        public Type BindingType => Binding.GetType();

        public string Value => Binding.GetProperty<OriginalStringBindingProperty>().Code;

        public Expression Expression => Binding.GetProperty<ParsedExpressionBindingProperty>(optional: true)?.Expression;

        public DataContextStack DataContextTypeStack => Binding.GetProperty<DataContextStack>();

        //public Exception ParsingError { get; set; }

        public ITypeDescriptor ResultType => new ResolvedTypeDescriptor(Binding.GetProperty<ResultTypeBindingProperty>(optional: true)?.Type);

        IDataContextStack IAbstractBinding.DataContextTypeStack => DataContextTypeStack;

        IAbstractTreeNode IAbstractTreeNode.Parent => Parent;

        //public DebugInfoExpression DebugInfo { get; set; }

        public ResolvedBinding(BindingCompilationService bindingService, Type bindingType, DataContextStack dataContext, string code = null, Expression parsedExpression = null, DotvvmProperty property = null)
        {
            var properties = new List<object> {
                dataContext,
                this
            };
            if (code != null) properties.Add(new OriginalStringBindingProperty(code));
            if (parsedExpression != null) properties.Add(new ParsedExpressionBindingProperty(parsedExpression));
            if (property != null) properties.Add(new AssignedPropertyBindingProperty(property));
            if (property == DotvvmBindableObject.DataContextProperty) properties.Add(IncludesThisDataContextBindingFlag.Instance);
            this.Binding = (IBinding)Activator.CreateInstance(bindingType, new object[] { bindingService, properties });
            this.BindingService = bindingService;
        }

        public Expression GetExpression() => Binding.GetProperty<ParsedExpressionBindingProperty>().Expression;

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitBinding(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
        }
    }
}
