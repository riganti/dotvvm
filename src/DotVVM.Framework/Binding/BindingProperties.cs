using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Immutable;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Binding.Properties
{
    public sealed class KnockoutExpressionBindingProperty
    {
        public readonly ParametrizedCode Code;
        public readonly ParametrizedCode UnwrapedCode;
        public KnockoutExpressionBindingProperty(ParametrizedCode code, ParametrizedCode unwrapedCode)
        {
            this.Code = code;
            this.UnwrapedCode = unwrapedCode;
        }
    }

    public sealed class OriginalStringBindingProperty
    {
        public readonly string Code;
        public OriginalStringBindingProperty(string code)
        {
            this.Code = code;
        }
    }

    public sealed class ResultTypeBindingProperty
    {
        public readonly Type Type;
        public ResultTypeBindingProperty(Type type)
        {
            this.Type = type;
        }
    }

    public sealed class IdBindingProperty
    {
        public readonly string Id;
        public IdBindingProperty(string id)
        {
            this.Id = id;
        }
    }

    public sealed class CommandJavascriptBindingProperty
    {
        public readonly ParametrizedCode Code;
        public CommandJavascriptBindingProperty(ParametrizedCode code)
        {
            this.Code = code;
        }
    }

    public sealed class StaticCommandJavascriptProperty
    {
        public readonly ParametrizedCode Code;
        public StaticCommandJavascriptProperty(ParametrizedCode code)
        {
            this.Code = code;
        }
    }

    public sealed class ParsedExpressionBindingProperty
    {
        public readonly Expression Expression;
        public ParsedExpressionBindingProperty(Expression expression)
        {
            this.Expression = expression;
        }
    }

    public sealed class CastedExpressionBindingProperty
    {
        public readonly Expression Expression;
        public CastedExpressionBindingProperty(Expression expression)
        {
            this.Expression = expression;
        }
    }

    public sealed class KnockoutJsExpressionBindingProperty
    {
        public readonly JsExpression Expression;
        public KnockoutJsExpressionBindingProperty(JsExpression expression)
        {
            this.Expression = expression;
        }
    }

    public sealed class ActionFiltersBindingProperty
    {
        public readonly ImmutableArray<IActionFilter> Filters;
        public ActionFiltersBindingProperty(ImmutableArray<IActionFilter> filters)
        {
            this.Filters = filters;
        }
    }
    public sealed class BindingAdditionalResolvers
    {
        public ImmutableArray<Delegate> Resolvers { get; }
        public BindingAdditionalResolvers(IEnumerable<Delegate> resolvers)
        {
            Resolvers = resolvers.ToImmutableArray();
        }
    }

    public sealed class ExpectedTypeBindingProperty
    {
        public readonly Type Type;
        public ExpectedTypeBindingProperty(Type type)
        {
            this.Type = type;
        }
    }

    public sealed class LocationInfoBindingProperty
    {
        public readonly string FileName;
        public readonly (int, int)[] Ranges;
        public readonly int LineNumber;
        public readonly Type ControlType;

        public LocationInfoBindingProperty(string fileName, (int, int)[] ranges, int lineNumber, Type controlType)
        {
            this.FileName = fileName;
            this.Ranges = ranges;
            this.LineNumber = lineNumber;
            this.ControlType = controlType;
        }
    }

    public sealed class IsMutableBindingProperty
    {
        public readonly bool IsMutable;
        public IsMutableBindingProperty(bool isMutable)
        {
            this.IsMutable = isMutable;
        }
    }

    public sealed class AssignedPropertyBindingProperty
    {
        public readonly DotvvmProperty DotvvmProperty;
        public AssignedPropertyBindingProperty(DotvvmProperty property)
        {
            this.DotvvmProperty = property;
        }
    }

    public sealed class BindingErrorReporterProperty
    {
        public ConcurrentStack<(Type req, Exception error, DiagnosticSeverity)> Errors = new ConcurrentStack<(Type req, Exception error, DiagnosticSeverity)>();
    }

    public sealed class DataSourceAccessBinding
    {
        public readonly IBinding Binding;
        public DataSourceAccessBinding(IBinding binding)
        {
            this.Binding = binding;
        }
    }

    public sealed class DataSourceCurrentElementBinding
    {
        public readonly IBinding Binding;
        public DataSourceCurrentElementBinding(IBinding binding)
        {
            this.Binding = binding;
        }
    }

    public sealed class DataSourceLengthBinding
    {
        public readonly IBinding Binding;
        public DataSourceLengthBinding(IBinding binding)
        {
            this.Binding = binding;
        }
    }
}
