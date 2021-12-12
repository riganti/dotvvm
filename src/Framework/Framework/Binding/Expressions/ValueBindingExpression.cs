using System;
using System.Collections;
using System.Collections.Generic;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Javascript.Ast;
using System.Reflection;
using DotVVM.Framework.Utils;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using System.Collections.Immutable;

namespace DotVVM.Framework.Binding.Expressions
{
    /// <summary>
    /// A binding that gets the value from a viewmodel property.
    /// </summary>
    [BindingCompilationRequirements(
        required: new[] {
            typeof(BindingDelegate),
            typeof(ResultTypeBindingProperty),
            typeof(KnockoutExpressionBindingProperty)
        },
        optional: new[] { typeof(BindingUpdateDelegate) })]
    [Options]
    public class ValueBindingExpression : BindingExpression, IUpdatableValueBinding, IValueBinding
    {
        public ValueBindingExpression(BindingCompilationService service, IEnumerable<object?> properties)
            : base(service, properties)
        {
            AddNullResolvers();
        }

        private protected MaybePropValue<KnockoutExpressionBindingProperty> knockoutExpressions;

        private protected override void StoreProperty(object p)
        {
            if (p is KnockoutExpressionBindingProperty knockoutExpressions)
                this.knockoutExpressions.SetValue(new(knockoutExpressions));
            else
                base.StoreProperty(p);
        }

        public override object? GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException)
        {
            if (type == typeof(KnockoutExpressionBindingProperty))
                return knockoutExpressions.GetValue(this).GetValue(errorMode, this, type);
            return base.GetProperty(type, errorMode);
        }

        private protected override IEnumerable<object?> GetOutOfDictionaryProperties() =>
            base.GetOutOfDictionaryProperties().Concat(new object?[] {
                knockoutExpressions.Value.Value
            });

        public BindingDelegate BindingDelegate => this.bindingDelegate.GetValueOrThrow(this);

        public BindingUpdateDelegate UpdateDelegate => this.updateDelegate.GetValueOrThrow(this);

        public KnockoutExpressionBindingProperty KnockoutExpressionBindingProperty => this.knockoutExpressions.GetValueOrThrow(this);

        public ParametrizedCode KnockoutExpression => KnockoutExpressionBindingProperty.Code;
        public ParametrizedCode UnwrappedKnockoutExpression => KnockoutExpressionBindingProperty.UnwrappedCode;
        public ParametrizedCode WrappedKnockoutExpression => KnockoutExpressionBindingProperty.WrappedCode;

        public Type ResultType => this.resultType.GetValueOrThrow(this).Type;

        public class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() => new Delegate[] {
                new Func<KnockoutJsExpressionBindingProperty, RequiredRuntimeResourcesBindingProperty>(js =>
                {
                    var resources =
                        js.Expression
                        .DescendantNodesAndSelf()
                        .Select(n => n.Annotation<RequiredRuntimeResourcesBindingProperty>())
                        .Where(n => n != null)
                        .SelectMany(n => n!.Resources)
                        .ToImmutableArray();

                    return resources.Length == 0
                        ? RequiredRuntimeResourcesBindingProperty.Empty
                        : new RequiredRuntimeResourcesBindingProperty(resources);
                }),
                new Func<KnockoutJsExpressionBindingProperty, GlobalizeResourceBindingProperty?>(js =>
                {
                    var isGlobalizeRequired = js.Expression.DescendantNodesAndSelf()
                        .Any(n => n.Annotation<GlobalizeResourceBindingProperty>() != null);
                    if (isGlobalizeRequired)
                    {
                        return new GlobalizeResourceBindingProperty();
                    }
                    return null;
                })
            };
        }

        #region Helpers

        /// Creates binding {value: _this} for a specific data context. Note that the result is cached (non-deterministically, using the <see cref="DotVVM.Framework.Runtime.Caching.IDotvvmCacheAdapter" />)
        public static ValueBindingExpression<T> CreateThisBinding<T>(BindingCompilationService service, DataContextStack dataContext) =>
            service.Cache.CreateCachedBinding("ValueBindingExpression.ThisBinding", new [] { dataContext }, () => CreateBinding<T>(service, o => (T)o[0]!, dataContext));

        /// Crates a new value binding expression from the specified .NET delegate and Javascript expression. Note that this operation is not very cheap and the result is not cached.
        public static ValueBindingExpression<T> CreateBinding<T>(BindingCompilationService service, Func<object?[], T> func, JsExpression expression, DataContextStack? dataContext = null) =>
            new ValueBindingExpression<T>(service, new object?[] {
                new BindingDelegate((o, c) => func(o)),
                new ResultTypeBindingProperty(typeof(T)),
                new KnockoutJsExpressionBindingProperty(expression),
                dataContext
            });

        /// Crates a new value binding expression from the specified .NET delegate and Javascript expression. Note that this operation is not very cheap and the result is not cached.
        public static ValueBindingExpression<T> CreateBinding<T>(BindingCompilationService service, Func<object?[], T> func, ParametrizedCode expression, DataContextStack? dataContext = null) =>
            new ValueBindingExpression<T>(service, new object?[] {
                new BindingDelegate((o, c) => func(o)),
                new ResultTypeBindingProperty(typeof(T)),
                new KnockoutExpressionBindingProperty(expression, expression, expression),
                dataContext
            });

        /// Crates a new value binding expression from the specified Linq.Expression. Note that this operation is quite expansive and the result is not cached (you are supposed to do it and NOT invoke this function for every request).
        public static ValueBindingExpression<T> CreateBinding<T>(BindingCompilationService service, Expression<Func<object?[], T>> expr, DataContextStack? dataContext)
        {
            var visitor = new ViewModelAccessReplacer(expr.Parameters.Single());
            var expression = visitor.Visit(expr.Body);
            dataContext = dataContext ?? visitor.GetDataContext();
            visitor.ValidateDataContext(dataContext);
            return new ValueBindingExpression<T>(service, new object?[] {
                new ParsedExpressionBindingProperty(BindingHelper.AnnotateStandardContextParams(expression, dataContext).OptimizeConstants()),
                new ResultTypeBindingProperty(typeof(T)),
                dataContext
            });
        }

        class ViewModelAccessReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression vmParameter;

            public ViewModelAccessReplacer(ParameterExpression vmParameter)
            {
                this.vmParameter = vmParameter;
            }
            private List<Type?> VmTypes { get; set; } = new List<Type?>();

            public DataContextStack GetDataContext()
            {
                DataContextStack? c = null;
                foreach (var vm in VmTypes)
                {
                    c = DataContextStack.Create(vm ?? typeof(object), c);
                }
                return c.NotNull();
            }

            public void ValidateDataContext(DataContextStack? dataContext)
            {
                for (int i = 0; i < VmTypes.Count; i++, dataContext = dataContext.Parent)
                {
                    var t = VmTypes[i];
                    if (dataContext == null) throw new Exception($"Cannot access _parent{i}, it does not exist in the data context.");
                    if (t != null && !t.IsAssignableFrom(dataContext.DataContextType))
                        throw new Exception($"_parent{i} does not have type '{t}' but '{dataContext.DataContextType}'.");
                }
            }

            public override Expression Visit(Expression node)
            {
                if (node.NodeType == ExpressionType.Convert && node is UnaryExpression unary &&
                    unary.Operand.NodeType == ExpressionType.ArrayIndex && unary.Operand is BinaryExpression indexer &&
                    indexer.Right is ConstantExpression indexConstant &&
                    indexer.Left == vmParameter)
                {
                    int index = (int)indexConstant.Value;
                    while (VmTypes.Count <= index) VmTypes.Add(null);
                    if (VmTypes[index]?.IsAssignableFrom(unary.Type) != true)
                    {
                        if (VmTypes[index] == null || unary.Type.IsAssignableFrom(VmTypes[index]))
                            VmTypes[index] = unary.Type;
                        else throw new Exception("Unsatisfiable view model type constraint");
                    }
                    return Expression.Parameter(unary.Type, "_parent" + index);
                }
                if (node == vmParameter) throw new NotSupportedException();
                return base.Visit(node);
            }
        }

        public IValueBinding GetListIndexer()
        {
            return (IValueBinding)this.GetProperty<DataSourceCurrentElementBinding>().Binding;
        }

        #endregion
    }

    public class ValueBindingExpression<T> : ValueBindingExpression, IValueBinding<T>, IUpdatableValueBinding<T>
    {
        public new BindingDelegate<T> BindingDelegate => base.BindingDelegate.ToGeneric<T>();

        public new BindingUpdateDelegate<T> UpdateDelegate => base.UpdateDelegate.ToGeneric<T>();

        public ValueBindingExpression(BindingCompilationService service, IEnumerable<object?> properties) : base(service, properties) { }
    }
}
