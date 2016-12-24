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

namespace DotVVM.Framework.Compilation
{
    public class BindingCompiler : IBindingCompiler
    {
        public static ConcurrentDictionary<int, CompiledBindingExpression> GlobalBindingList = new ConcurrentDictionary<int, CompiledBindingExpression>();
        private static int globalBindingIndex = 0;
     
        private DotvvmConfiguration configuration;

        public BindingCompiler(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public static BindingCompilationAttribute GetCompilationAttribute(Type bindingType)
        {
            return bindingType.GetTypeInfo().GetCustomAttributes<BindingCompilationAttribute>().FirstOrDefault();
        }

        private static KeyValuePair<string, Expression> GetParameter(int index, string name, Expression vmArray, Type[] parents)
        {
            return new KeyValuePair<string, Expression>(name, Expression.Convert(Expression.ArrayIndex(vmArray, Expression.Constant(index)), parents[index]));
        }

        public static IEnumerable<KeyValuePair<string, Expression>> GetParameters(DataContextStack dataContext, Expression vmArray, Expression controlRoot)
        {
            var par = dataContext.Enumerable().ToArray();
            yield return GetParameter(0, ParserConstants.ThisSpecialBindingProperty, vmArray, par);
            yield return GetParameter(par.Length - 1, ParserConstants.RootSpecialBindingProperty, vmArray, par);
            yield return new KeyValuePair<string, Expression>("_control", controlRoot);
            yield return new KeyValuePair<string, Expression>("_parents", vmArray);
            yield return new KeyValuePair<string, Expression>("_page", Expression.New(typeof(BindingPageInfo)));
            if (par.Length > 0)
            {
                if (par.Length > 1)
                    yield return GetParameter(1, ParserConstants.ParentSpecialBindingProperty, vmArray, par);
                for (int i = 0; i < par.Length; i++)
                {
                    yield return GetParameter(i, ParserConstants.ParentSpecialBindingProperty + i, vmArray, par);
                }
            }
        }

        public static T TryExecute<T>(DothtmlNode node, string errorMessage, BindingCompilationRequirementType requirement, Func<T> action)
        {
#if !DEBUG
            if (requirement == BindingCompilationRequirementType.No) return default(T);
#endif
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                if (requirement != BindingCompilationRequirementType.StronglyRequire) return default(T);
                else if (ex is DotvvmCompilationException) throw;
                else throw new DotvvmCompilationException(errorMessage, ex, node.Tokens);
            }
        }

        public virtual ExpressionSyntax EmitCreateBinding(DefaultViewCompilerCodeEmitter emitter, ResolvedBinding binding, string id, Type expectedType)
        {
            var compilerAttribute = GetCompilationAttribute(binding.BindingType);
            var requirements = compilerAttribute.GetRequirements(binding.BindingType);

            var expression = new Lazy<Expression>(() => compilerAttribute.GetExpression(binding));
            var compiled = new CompiledBindingExpression();
            compiled.Delegate = TryExecute(binding.BindingNode, "Error while compiling binding to delegate.", requirements.Delegate, () => CompileExpression(compilerAttribute.CompileToDelegate(binding.GetExpression(), binding.DataContextTypeStack, expectedType), binding.DebugInfo));
            compiled.UpdateDelegate = TryExecute(binding.BindingNode, "Error while compiling update delegate.", requirements.UpdateDelegate, () => compilerAttribute.CompileToUpdateDelegate(binding.GetExpression(), binding.DataContextTypeStack).Compile());
            compiled.OriginalString = TryExecute(binding.BindingNode, "hey, no, that should not happen. Really.", requirements.OriginalString, () => binding.Value);
            compiled.Expression = TryExecute(binding.BindingNode, "Could not get binding expression.", requirements.Expression, () => binding.GetExpression());
            compiled.Id = id;
            compiled.ActionFilters = TryExecute(binding.BindingNode, "", requirements.ActionFilters, () => compilerAttribute.GetActionFilters(binding.GetExpression()).ToArray());

            compiled.Javascript = TryExecute(binding.BindingNode, "Could not compile binding to Javascript.", requirements.Javascript, () => compilerAttribute.CompileToJavascript(binding, compiled, configuration));

            var index = Interlocked.Increment(ref globalBindingIndex);
            if (!GlobalBindingList.TryAdd(index, compiled))
                throw new Exception("internal bug");
            return EmitGetCompiledBinding(index);
        }

        private T CompileExpression<T>(Expression<T> expression, DebugInfoExpression debugInfo)
        {
            if(!configuration.Debug || !configuration.AllowBindingDebugging || debugInfo == null)
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


        protected virtual ExpressionSyntax EmitGetCompiledBinding(int index)
        {
            return SyntaxFactory.ElementAccessExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.ParseTypeName($"global::{typeof(BindingCompiler).FullName}"),
                    SyntaxFactory.IdentifierName(nameof(GlobalBindingList))
                ),
                SyntaxFactory.BracketedArgumentList(
                    SyntaxFactory.SeparatedList(
                        new[]
                        {
                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(index)))
                        }
                    )
                )
            );
        }
    }
}
