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

        public static BindingCompilationAttribute GetCompilationAttribute(Type bindingType)
        {
            return bindingType.GetCustomAttributes<BindingCompilationAttribute>().FirstOrDefault();
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
                else throw;
            }
        }

        public virtual ExpressionSyntax EmitCreateBinding(ResolvedBinding binding, string id, Type expectedType)
        {
            var compilerAttribute = GetCompilationAttribute(binding.BindingType);
            var requirements = compilerAttribute.GetRequirements(binding.BindingType);

            var expression = new Lazy<Expression>(() => compilerAttribute.GetExpression(binding));
            var compiled = new CompiledBindingExpression();
            compiled.Delegate = TryExecute(requirements.Delegate, () => compilerAttribute.CompileToDelegate(binding.GetExpression(), binding.DataContextTypeStack, expectedType).Compile());
            compiled.UpdateDelegate = TryExecute(requirements.UpdateDelegate, () => compilerAttribute.CompileToUpdateDelegate(binding.GetExpression(), binding.DataContextTypeStack).Compile());
            compiled.OriginalString = TryExecute(requirements.OriginalString, () => binding.Value);
            compiled.Expression = TryExecute(requirements.Expression, () => binding.GetExpression());
            compiled.Id = id;
            compiled.ActionFilters = TryExecute(requirements.ActionFilters, () => compilerAttribute.GetActionFilters(binding.GetExpression()).ToArray());

            compiled.Javascript = TryExecute(requirements.Javascript, () => compilerAttribute.CompileToJs(binding, compiled));

            var index = Interlocked.Increment(ref globalBindingIndex);
            if (!GlobalBindingList.TryAdd(index, compiled))
                throw new Exception("internal bug");
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
