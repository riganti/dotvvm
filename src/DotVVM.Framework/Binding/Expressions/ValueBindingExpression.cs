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
        public ValueBindingExpression(BindingCompilationService service, IEnumerable<object> properties) 
            : base(service, properties)
        {
            AddNullResolvers();
        }

        public BindingDelegate BindingDelegate => this.GetProperty<BindingDelegate>();

        public BindingUpdateDelegate UpdateDelegate => this.GetProperty<BindingUpdateDelegate>();

        public ParametrizedCode KnockoutExpression => this.GetProperty<KnockoutExpressionBindingProperty>().Code;
        public ParametrizedCode UnwrappedKnockoutExpression => this.GetProperty<KnockoutExpressionBindingProperty>().UnwrappedCode;
        public ParametrizedCode WrappedKnockoutExpression => this.GetProperty<KnockoutExpressionBindingProperty>().WrappedCode;

        public Type ResultType => this.GetProperty<ResultTypeBindingProperty>().Type;

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
                        .SelectMany(n => n.Resources)
                        .ToImmutableArray();

                    return resources.Length == 0
                        ? RequiredRuntimeResourcesBindingProperty.Empty
                        : new RequiredRuntimeResourcesBindingProperty(resources);
                })
            };
        }

        #region Helpers

        public static ValueBindingExpression<T> CreateThisBinding<T>(BindingCompilationService service, DataContextStack dataContext) =>
            CreateBinding<T>(service, o => (T)o[0], dataContext);

        public static ValueBindingExpression<T> CreateBinding<T>(BindingCompilationService service, Func<object[], T> func, JsExpression expression, DataContextStack dataContext = null) =>
            new ValueBindingExpression<T>(service, new object[] {
                new BindingDelegate((o, c) => func(o)),
                new ResultTypeBindingProperty(typeof(T)),
                new KnockoutJsExpressionBindingProperty(expression),
                dataContext
            });

        public static ValueBindingExpression<T> CreateBinding<T>(BindingCompilationService service, Func<object[], T> func, ParametrizedCode expression, DataContextStack dataContext = null) =>
            new ValueBindingExpression<T>(service, new object[] {
                new BindingDelegate((o, c) => func(o)),
                new ResultTypeBindingProperty(typeof(T)),
                new KnockoutExpressionBindingProperty(expression, expression, expression),
                dataContext
            });

        public static ValueBindingExpression<T> CreateBinding<T>(BindingCompilationService service, Expression<Func<object[], T>> expr, DataContextStack dataContext)
        {
            var visitor = new ViewModelAccessReplacer(expr.Parameters.Single());
            var expression = visitor.Visit(expr.Body);
            dataContext = dataContext ?? visitor.GetDataContext();
            return new ValueBindingExpression<T>(service, new object[] {
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
            private List<Type> VmTypes { get; set; } = new List<Type>();

            public DataContextStack GetDataContext()
            {
                DataContextStack c = null;
                foreach (var vm in VmTypes)
                {
                    c = DataContextStack.Create(vm ?? typeof(object), c);
                }
                return c;
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

        public ValueBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }
    }
}