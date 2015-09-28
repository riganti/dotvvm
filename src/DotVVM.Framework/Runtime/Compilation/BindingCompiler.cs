using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using System.Collections.Concurrent;
using DotVVM.Framework.Binding;
using System.Reflection;
using System.Linq.Expressions;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Controls;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotVVM.Framework.Runtime.Filters;
using System.Diagnostics;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class BindingCompiler : IBindingCompiler
    {
        public static ConcurrentDictionary<int, CompiledBindingExpression> GlobalBindingList = new ConcurrentDictionary<int, CompiledBindingExpression>();
        private static int globalBindingIndex = 0;
        private static ConcurrentDictionary<Type, BindingCompilationRequirementsAttribute> requirementCache = new ConcurrentDictionary<Type, BindingCompilationRequirementsAttribute>();

        public static BindingCompilationRequirementsAttribute GetRequirements(Type bindingType)
        {
            return requirementCache.GetOrAdd(bindingType, _ =>
            {
                return bindingType.GetCustomAttribute<BindingCompilationRequirementsAttribute>(true) ?? new BindingCompilationRequirementsAttribute
                {
                    Delegate = BindingCompilationRequirementType.IfPossible,
                    OriginalString = BindingCompilationRequirementType.IfPossible,
                    Expression = BindingCompilationRequirementType.IfPossible,
                    Javascript = BindingCompilationRequirementType.IfPossible
                };
            });
        }

        public static CompileJavascriptAttribute GetJsCompiler(Type bindingType)
        {
            return bindingType.GetCustomAttributes<CompileJavascriptAttribute>().FirstOrDefault();
        }

        private static KeyValuePair<string, Expression> GetParameter(int index, string name, Expression vmArray, Type[] parents)
        {
            return new KeyValuePair<string, Expression>(name, Expression.Convert(Expression.ArrayIndex(vmArray, Expression.Constant(index)), parents[index]));
        }

        public static IEnumerable<KeyValuePair<string, Expression>> GetParameters(DataContextStack dataContext, Expression vmArray, Expression controlRoot)
        {
            var par = dataContext.Enumerable().ToArray();
            yield return GetParameter(0, Constants.ThisSpecialBindingProperty, vmArray, par);
            yield return GetParameter(par.Length - 1, Constants.RootSpecialBindingProperty, vmArray, par);
            yield return new KeyValuePair<string, Expression>("_control", controlRoot);
            yield return new KeyValuePair<string, Expression>("_parents", vmArray);
            if (par.Length > 0)
            {
                if (par.Length > 1)
                    yield return GetParameter(1, Constants.ParentSpecialBindingProperty, vmArray, par);
                for (int i = 1; i < par.Length; i++)
                {
                    yield return GetParameter(i, Constants.ParentSpecialBindingProperty + i, vmArray, par);
                }
            }
        }

        public static T TryExecute<T>(BindingCompilationRequirementType requirement, Func<T> action)
        {
            if (requirement == BindingCompilationRequirementType.No) return default(T);
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                if (requirement == BindingCompilationRequirementType.IfPossible) return default(T);
                else throw new InvalidOperationException("binding compilation failed", ex);
            }
        }

        public static Expression<CompiledBindingExpression.BindingDelegate> CompileToDelegate(Expression binding, DataContextStack dataContext)
        {
            var viewModelsParameter = Expression.Parameter(typeof(object[]), "vm");
            var controlRootParameter = Expression.Parameter(typeof(DotvvmControl), "controlRoot");
            var expr = ExpressionUtils.Replace(binding, GetParameters(dataContext, viewModelsParameter, Expression.Convert(controlRootParameter, dataContext.RootControlType)));
            expr = ExpressionUtils.ConvertToObject(expr);
            return Expression.Lambda<CompiledBindingExpression.BindingDelegate>(expr, viewModelsParameter, controlRootParameter);
        }

        public static Expression<CompiledBindingExpression.BindingUpdateDelegate> CompileToUpdateDelegate(Expression binding, DataContextStack dataContext)
        {
            var viewModelsParameter = Expression.Parameter(typeof(object[]), "vm");
            var controlRootParameter = Expression.Parameter(typeof(DotvvmControl), "controlRoot");
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var expr = ExpressionUtils.Replace(binding, GetParameters(dataContext, viewModelsParameter, Expression.Convert(controlRootParameter, dataContext.RootControlType)));
            var assignment = Expression.Assign(expr, Expression.Convert(valueParameter, expr.Type));
            return Expression.Lambda<CompiledBindingExpression.BindingUpdateDelegate>(assignment, viewModelsParameter, controlRootParameter, valueParameter);
        }

        public List<ActionFilterAttribute> GetActionFilters(Expression expression)
        {
            var list = new List<ActionFilterAttribute>();
            expression.ForEachMember(m =>
            {
                list.AddRange(m.GetCustomAttributes<ActionFilterAttribute>());
            });
            return list;
        }

        public virtual ExpressionSyntax EmitCreateBinding(DefaultViewCompilerCodeEmitter emitter, ResolvedBinding binding, string id)
        {
            var requirements = GetRequirements(binding.BindingType);
            var compiled = new CompiledBindingExpression();
            compiled.Delegate = TryExecute(requirements.Delegate, () => CompileToDelegate(binding.GetExpression(), binding.DataContextTypeStack).Compile());
            compiled.UpdateDelegate = TryExecute(requirements.UpdateDelegate, () => CompileToUpdateDelegate(binding.GetExpression(), binding.DataContextTypeStack).Compile());
            compiled.OriginalString = TryExecute(requirements.OriginalString, () => binding.Value);
            compiled.Expression = TryExecute(requirements.Expression, () => binding.GetExpression());
            compiled.Id = id;
            compiled.ActionFilters = TryExecute(requirements.ActionFilters, () => GetActionFilters(binding.GetExpression()).ToArray());

            var jsCompiler = GetJsCompiler(binding.BindingType);
            compiled.Javascript = TryExecute(requirements.Javascript, () => jsCompiler.CompileToJs(binding, compiled));

            var index = Interlocked.Increment(ref globalBindingIndex);
            if (!GlobalBindingList.TryAdd(index, compiled))
                throw new Exception("WTF");
            return EmitGetCompiledBinding(index);
        }

        protected virtual ExpressionSyntax EmitGetCompiledBinding(int index)
        {
            return SyntaxFactory.ElementAccessExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.ParseTypeName(typeof(BindingCompiler).FullName),
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
