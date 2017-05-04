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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Controls;

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
                        // T+ inherited dataContext
                        return ann.ExtensionParameter.GetServerEquivalent(CurrentControlParameter);
                    }
                    else
                    {
                        return Expression.Convert(Expression.ArrayIndex(ViewModelsParameter, Expression.Constant(ContextMap[ann.DataContext])), ann.DataContext.DataContextType);
                    }
                }
                return base.Visit(node);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (AssertAllReplaced && node != CurrentControlParameter && node != ViewModelsParameter)
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
            var requirements = binding.BindingService.GetRequirements(binding.Binding);

            var properties = requirements.Required.Concat(requirements.Optional)
                    .Concat(new[] { typeof(OriginalStringBindingProperty), typeof(DataContextStack), typeof(LocationInfoBindingProperty), typeof(BindingParserOptions) })
                    .Select(p => binding.Binding.GetProperty(p, ErrorHandlingMode.ReturnNull))
                    .Where(p => p != null).ToArray();
            return (IBinding)Activator.CreateInstance(binding.BindingType, new object[] {
                binding.BindingService,
                properties
            });
        }

        public virtual ExpressionSyntax EmitCreateBinding(DefaultViewCompilerCodeEmitter emitter, ResolvedBinding binding)
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
            public DebugInfoExpression DebugInfo { get; set; }
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                node = node.Update(Expression.Block(DebugInfo, node.Body), node.Parameters);
                return base.VisitLambda<T>(node);
            }
        }
    }
}
