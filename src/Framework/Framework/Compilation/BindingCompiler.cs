using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Controls;
using System.Diagnostics;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation
{
    public class BindingCompiler : IBindingCompiler
    {
        public static readonly ParameterExpression CurrentControlParameter = Expression.Parameter(typeof(DotvvmBindableObject), "currentControl");
        public static readonly ParameterExpression ViewModelsParameter = Expression.Parameter(typeof(object[]), "vm");

        protected readonly DotvvmConfiguration configuration;

        public BindingCompiler(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public static Expression ReplaceParameters(Expression expression, DataContextStack dataContext, bool assertAllReplaced = true) =>
            new ParameterReplacementVisitor(dataContext, assertAllReplaced).Visit(expression);

        class ParameterReplacementVisitor: ExpressionVisitor
        {
            private readonly Dictionary<DataContextStack, int> ContextMap;
            private readonly bool AssertAllReplaced;
            private readonly HashSet<ParameterExpression> contextParameters = new HashSet<ParameterExpression>();

            public ParameterReplacementVisitor(DataContextStack dataContext, bool assertAllReplaced = true)
            {
                this.ContextMap = dataContext.EnumerableItems().Select((a, i) => (a, i)).ToDictionary(a => a.Item1, a => a.Item2);
                this.AssertAllReplaced = assertAllReplaced;
            }

            public override Expression Visit(Expression node)
            {
                if (node?.GetParameterAnnotation() is BindingParameterAnnotation ann)
                {
                    if (ann.ExtensionParameter != null)
                    {
                        // handle data context hierarchy
                        var friendlyIdentifier = $"extension parameter {ann.ExtensionParameter.Identifier}";
                        var targetControl =
                            ann.DataContext is null || ContextMap[ann.DataContext] == 0
                                ? CurrentControlParameter
                                : ExpressionUtils.Replace(
                                    (DotvvmBindableObject control) => BindingHelper.FindDataContextTarget(control, ann.DataContext, friendlyIdentifier).target,
                                    CurrentControlParameter
                                ).OptimizeConstants();

                        return ann.ExtensionParameter.GetServerEquivalent(targetControl);
                    }
                    else
                    {
                        var dc = ann.DataContext.NotNull("Invalid BindingParameterAnnotation");
                        return Expression.Convert(Expression.ArrayIndex(ViewModelsParameter, Expression.Constant(ContextMap[dc])), dc.DataContextType);
                    }
                }
                return base.Visit(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> expr)
            {
                var currentParameters = expr.Parameters.Where(contextParameters.Add).ToList();
                try {
                    return base.VisitLambda(expr);
                }
                finally {
                    Debug.Assert(currentParameters.TrueForAll(contextParameters.Remove));
                }
            }

            protected override Expression VisitBlock(BlockExpression expr)
            {
                var currentParameters = expr.Variables.Where(contextParameters.Add).ToList();
                try {
                    return base.VisitBlock(expr);
                }
                finally {
                    Debug.Assert(currentParameters.TrueForAll(contextParameters.Remove));
                }
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (AssertAllReplaced && node != CurrentControlParameter && node != ViewModelsParameter && !contextParameters.Contains(node))
                    throw new Exception($"Parameter {node.Name}:{node.Type.Name} could not be translated.");
                return base.VisitParameter(node);
            }
        }

        private static KeyValuePair<string, Expression> GetParameter(int index, string name, Expression vmArray, Type[] parents)
        {
            return new KeyValuePair<string, Expression>(name, Expression.Convert(Expression.ArrayIndex(vmArray, Expression.Constant(index)), parents[index]));
        }

        public virtual IBinding CreateMinimalClone(ResolvedBinding binding)
        {
            var properties = GetMinimalCloneProperties(binding.Binding);
            return (IBinding)Activator.CreateInstance(binding.BindingType, new object[] {
                binding.BindingService,
                properties
            });
        }

        public static IEnumerable<object> GetMinimalCloneProperties(IBinding binding)
        {
            var requirements = binding.GetProperty<BindingCompilationService>().GetRequirements(binding);
            return requirements.Required.Concat(requirements.Optional)
                    .Concat(new[] { typeof(ParsedExpressionBindingProperty), typeof(OriginalStringBindingProperty), typeof(DataContextStack), typeof(DotvvmLocationInfo), typeof(BindingParserOptions), typeof(BindingCompilationRequirementsAttribute), typeof(ExpectedTypeBindingProperty), typeof(AssignedPropertyBindingProperty) })
                    .Select(p => binding.GetProperty(p, ErrorHandlingMode.ReturnNull))
                    .Where(p => p != null).ToArray()!;
        }

        public virtual Expression EmitCreateBinding(DefaultViewCompilerCodeEmitter emitter, ResolvedBinding binding)
        {
            var newbinding = CreateMinimalClone(binding);
            return emitter.EmitValue(newbinding);
        }

        private T CompileExpression<T>(Expression<T> expression, DebugInfoExpression debugInfo)
        {
            if (!configuration.Debug || !configuration.AllowBindingDebugging || debugInfo == null)
            {
                return expression.Compile();
            }
            else
            {
                throw new NotImplementedException();
                //try
                //{
                //    var visitor = new DebugInfoExpressionVisitor { DebugInfo = debugInfo };
                //    expression = visitor.Visit(expression) as Expression<T>;

                //    var pdb = DebugInfoGenerator.CreatePdbGenerator();
                //    //return expression.Compile(pdb);
                //    var type = moduleBuilder.Value.DefineType("bindingWrapperType" + Interlocked.Increment(ref bindingClassCtr));
                //    var method = type.DefineMethod("Method", MethodAttributes.Public | MethodAttributes.Static);
                //    expression.CompileToMethod(method, pdb);
                //    var bakedType = type.CreateType();
                //    return (T)(object)bakedType.GetMethods().First().CreateDelegate(typeof(T));
                //}
                //catch
                //{
                //    return expression.Compile();
                //}
            }
        }

        class DebugInfoExpressionVisitor : ExpressionVisitor
        {
            public DebugInfoExpression DebugInfo { get; }

            public DebugInfoExpressionVisitor(DebugInfoExpression debugInfo)
            {
                DebugInfo = debugInfo;
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                node = node.Update(Expression.Block(DebugInfo, node.Body), node.Parameters);
                return base.VisitLambda<T>(node);
            }
        }
    }
}
