#nullable enable
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
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Binding.Properties
{
    /// <summary>
    /// Contains evaluable Javascript in knockout binding, knockout context parameters are represented as symbolic parameters in the ParametrizedCode
    /// </summary>
    public sealed class KnockoutExpressionBindingProperty
    {
        /// <summary>
        /// Knockout binding expression. May return observable.
        /// </summary>
        public readonly ParametrizedCode Code;
        /// <summary>
        /// Knockout binding expression. Always unwraps the observable.
        /// </summary>
        public readonly ParametrizedCode UnwrappedCode;
        /// Knockout binding expression. Always returns an observable.
        public readonly ParametrizedCode WrappedCode;
        public KnockoutExpressionBindingProperty(ParametrizedCode code, ParametrizedCode unwrappedCode, ParametrizedCode wrappedCode)
        {
            this.Code = code;
            this.UnwrappedCode = unwrappedCode;
            this.WrappedCode = wrappedCode;
        }
    }

    /// <summary>
    /// Contains string that identifies the translated binding.
    /// </summary>
    public sealed class SimplePathExpressionBindingProperty
    {
        public readonly ParametrizedCode Code;
        public SimplePathExpressionBindingProperty(ParametrizedCode code)
        {
            this.Code = code;
        }
    }

    /// <summary>
    /// Contains original binding string, as it was typed in dothtml file. (it is trimmed)
    /// </summary>
    public sealed class OriginalStringBindingProperty
    {
        public readonly string Code;
        public OriginalStringBindingProperty(string code)
        {
            this.Code = code;
        }
    }

    /// <summary>
    /// Contains binding's result type.
    /// </summary>
    public sealed class ResultTypeBindingProperty
    {
        public readonly Type Type;
        public ResultTypeBindingProperty(Type type)
        {
            this.Type = type;
        }
    }

    /// <summary>
    /// Contains unique id of binding in its DataContext and the page
    /// </summary>
    public sealed class IdBindingProperty
    {
        public readonly string Id;
        public IdBindingProperty(string id)
        {
            this.Id = id;
        }
    }

    /// <summary>
    /// Contains JS code, that will invoke the command. May contain symbolic parameters from `JavascriptTranslator` and `CommandBindingExpression`
    /// </summary>
    public sealed class CommandJavascriptBindingProperty
    {
        public readonly ParametrizedCode Code;
        public CommandJavascriptBindingProperty(ParametrizedCode code)
        {
            this.Code = code;
        }
    }

    /// <summary>
    /// Contains JS code, that will invoke the static command. May contain symbolic parameters from `JavascriptTranslator` and `CommandBindingExpression`
    /// </summary>
    public sealed class StaticCommandJavascriptProperty
    {
        public readonly ParametrizedCode Code;
        public StaticCommandJavascriptProperty(ParametrizedCode code)
        {
            this.Code = code;
        }
    }

    /// <summary>
    /// Contains JS code, that will invoke the static command. May contain symbolic parameters from `JavascriptTranslator` and `CommandBindingExpression`
    /// </summary>
    public sealed class StaticCommandJsAstProperty
    {
        public readonly JsExpression Expression;
        public StaticCommandJsAstProperty(JsExpression expression)
        {
            this.Expression = expression;
        }
    }

    /// <summary>
    /// Contains <see cref="System.Linq.Expressions.Expression"/> instance that represents code as it was written in markup with minimal processing.
    /// </summary>
    public sealed class ParsedExpressionBindingProperty
    {
        public readonly Expression Expression;
        public ParsedExpressionBindingProperty(Expression expression)
        {
            this.Expression = expression;
        }
    }

    /// <summary>
    /// Contains <see cref="System.Linq.Expressions.Expression"/> instance that represents code converted to be evaluated as binding (type conversions applied, ...). 
    /// </summary>
    public sealed class CastExpressionBindingProperty
    {
        public readonly Expression Expression;
        public CastExpressionBindingProperty(Expression expression)
        {
            this.Expression = expression;
        }
    }

    /// <summary>
    /// Contains raw translated JS AST that came from JavascriptTranslator. Specifically it has type annotations on it and does not include observable unwraps and null-checks.
    /// </summary>
    public sealed class KnockoutJsExpressionBindingProperty
    {
        public readonly JsExpression Expression;
        public KnockoutJsExpressionBindingProperty(JsExpression expression)
        {
            this.Expression = expression;
        }
    }

    /// <summary>
    /// Contains action filters that should be invoked before the binding invocation.
    /// </summary>
    public sealed class ActionFiltersBindingProperty
    {
        public readonly ImmutableArray<IActionFilter> Filters;
        public ActionFiltersBindingProperty(ImmutableArray<IActionFilter> filters)
        {
            this.Filters = filters;
        }
    }

    /// <summary>
    /// Contains expected type of the binding - typically type of the bound property.
    /// </summary>
    public sealed class ExpectedTypeBindingProperty
    {
        public readonly Type Type;
        public ExpectedTypeBindingProperty(Type type)
        {
            this.Type = type;
        }
    }

    /// <summary>
    /// Contains debug information about original binding location.
    /// </summary>
    public sealed class LocationInfoBindingProperty
    {
        public readonly string? FileName;
        public readonly (int, int)[]? Ranges;
        public readonly int LineNumber;
        public readonly Type? ControlType;
        public readonly DotvvmProperty? RelatedProperty;

        public LocationInfoBindingProperty(string fileName, (int, int)[] ranges, int lineNumber, Type controlType, DotvvmProperty? relatedProperty = null)
        {
            this.FileName = fileName;
            this.Ranges = ranges;
            this.LineNumber = lineNumber;
            this.ControlType = controlType;
            this.RelatedProperty = relatedProperty;
        }
    }

    /// <summary>
    /// Contains the property where the binding is assigned.
    /// </summary>
    public sealed class AssignedPropertyBindingProperty
    {
        public readonly DotvvmProperty DotvvmProperty;
        public AssignedPropertyBindingProperty(DotvvmProperty property)
        {
            this.DotvvmProperty = property;
        }
    }

    /// <summary>
    /// Contains (mutable) list of error that are produced during the binding lifetime.
    /// </summary>
    public sealed class BindingErrorReporterProperty
    {
        public ConcurrentStack<(Type req, Exception error, DiagnosticSeverity severity)> Errors = new ConcurrentStack<(Type req, Exception error, DiagnosticSeverity)>();
        public bool HasErrors => Errors.Any(e => e.severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        public string GetErrorMessage(IBinding binding)
        {
            var badRequirements = Errors.Where(e => e.severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Select(e => e.req).Distinct().ToArray();
            return $"Could not initialize binding '{binding}', requirement{(badRequirements.Length > 1 ? "s" : "")} {string.Join<Type>(", ", badRequirements)} {(badRequirements.Length > 1 ? "were" : "was")} not met.";
        }
        public IEnumerable<Exception> Exceptions => Errors.Select(e => e.error);
    }

    /// <summary>
    /// Contains a binding that unwraps <see cref="Controls.IBaseGridViewDataSet.Items"/>
    /// </summary>
    public sealed class DataSourceAccessBinding
    {
        public readonly IBinding Binding;
        public DataSourceAccessBinding(IBinding binding)
        {
            this.Binding = binding;
        }
    }

    /// <summary>
    /// Contains a binding that accesses $index-th element in the collection. Uses the <see cref="CurrentCollectionIndexExtensionParameter"/>.
    /// </summary>
    public sealed class DataSourceCurrentElementBinding
    {
        public readonly IBinding Binding;
        public DataSourceCurrentElementBinding(IBinding binding)
        {
            this.Binding = binding;
        }
    }

    /// <summary>
    /// Contains a binding that gets the collection's Length or Count
    /// </summary>
    public sealed class DataSourceLengthBinding
    {
        public readonly IBinding Binding;
        public DataSourceLengthBinding(IBinding binding)
        {
            this.Binding = binding;
        }
    }

    public sealed class SelectorItemBindingProperty
    {
        public readonly IValueBinding Expression;
        public SelectorItemBindingProperty(IValueBinding expression)
        {
            this.Expression = expression;
        }
    }

    public sealed class RequiredRuntimeResourcesBindingProperty
    {
        public readonly ImmutableArray<string> Resources;
        public RequiredRuntimeResourcesBindingProperty(ImmutableArray<string> resources)
        {
            this.Resources = resources;
        }
        public static readonly RequiredRuntimeResourcesBindingProperty Empty = new RequiredRuntimeResourcesBindingProperty(ImmutableArray<string>.Empty);
    }

    public sealed class GlobalizeResourceBindingProperty
    {
    }

    public sealed class ThisBindingProperty
    {
        public readonly IBinding binding;
        public ThisBindingProperty(IBinding binding)
        {
            this.binding = binding;
        }
    }

    public sealed class CollectionElementDataContextBindingProperty
    {
        public readonly DataContextStack DataContext;
        public CollectionElementDataContextBindingProperty(DataContextStack dataContext)
        {
            this.DataContext = dataContext;
        }
    }

    public sealed class IsMoreThanZeroBindingProperty
    {
        public readonly IBinding Binding;
        public IsMoreThanZeroBindingProperty(IBinding binding)
        {
            this.Binding = binding;
        }
    }

    public sealed class NegatedBindingExpression
    {
        public readonly IBinding Binding;
        public NegatedBindingExpression(IBinding binding)
        {
            this.Binding = binding;
        }
    }
    public sealed class ExpectedAsStringBindingExpression
    {
        public readonly IBinding Binding;
        public ExpectedAsStringBindingExpression(IBinding binding)
        {
            this.Binding = binding;
        }
    }
}
