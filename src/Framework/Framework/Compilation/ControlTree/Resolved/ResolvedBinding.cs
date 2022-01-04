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
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    [DebuggerDisplay("{BindingType.Name}: {Value}")]
    public class ResolvedBinding : ResolvedTreeNode, IAbstractBinding
    {
        public IBinding Binding {get;}

        public BindingCompilationService BindingService { get; }

        public DothtmlBindingNode? BindingNode => (DothtmlBindingNode?)DothtmlNode;

        public Type BindingType => Binding.GetType();
        public BindingParserOptions ParserOptions => Binding.GetProperty<BindingParserOptions>();

        public string Value => Binding.GetProperty<OriginalStringBindingProperty>().Code;

        public Expression? Expression => Binding.GetProperty<ParsedExpressionBindingProperty>(ErrorHandlingMode.ReturnNull)?.Expression;

        public DataContextStack DataContextTypeStack => Binding.DataContext.NotNull();

        public BindingErrorReporterProperty Errors => Binding.GetProperty<BindingErrorReporterProperty>();

        public ITypeDescriptor? ResultType => ResolvedTypeDescriptor.Create(Binding.GetProperty<ResultTypeBindingProperty>(ErrorHandlingMode.ReturnNull)?.Type);

        IDataContextStack IAbstractBinding.DataContextTypeStack => DataContextTypeStack;

        IAbstractTreeNode? IAbstractTreeNode.Parent => Parent;


        public ResolvedBinding(BindingCompilationService bindingService, BindingParserOptions bindingOptions, DataContextStack dataContext, string? code = null, Expression? parsedExpression = null, DotvvmProperty? property = null)
        {
            var bindingType = bindingOptions.BindingType;
            var properties = new List<object> {
                dataContext,
                this,
                bindingOptions,
                new BindingErrorReporterProperty()
            };
            if (code != null) properties.Add(new OriginalStringBindingProperty(code));
            if (parsedExpression != null) properties.Add(new ParsedExpressionBindingProperty(parsedExpression));
            if (property != null) properties.Add(new AssignedPropertyBindingProperty(property));
            this.BindingService = bindingService;
            this.Binding = bindingService.CreateBinding(bindingType, properties.ToArray());
        }

        public ResolvedBinding(IBinding binding)
        {
            this.Binding = binding;
            this.BindingService = binding.GetProperty<BindingCompilationService>();
        }

        public ResolvedBinding WithDifferentExpression(Expression expression, DotvvmProperty? property = null)
        {
            return new ResolvedBinding(
                BindingService,
                ParserOptions,
                DataContextTypeStack,
                code: null,
                expression,
                property ?? Binding.GetProperty<AssignedPropertyBindingProperty>(ErrorHandlingMode.ReturnNull)?.DotvvmProperty) { DothtmlNode = DothtmlNode };
        }

        public Expression GetExpression() => Binding.GetProperty<ParsedExpressionBindingProperty>().Expression;

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitBinding(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
        }

        public override string ToString() => Binding.ToString();
    }
}
