using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Binding
{
    public class BindingExpressionBuilder : IBindingExpressionBuilder
    {

        public Expression Parse(string expression, DataContextStack dataContexts, BindingParserOptions options)
        {
            try
            {
                var tokenizer = new BindingTokenizer();
                tokenizer.Tokenize(new StringReader(expression));

                var parser = new BindingParser();
                parser.Tokens = tokenizer.Tokens;
                var node = parser.ReadExpression();
                if (!parser.OnEnd())
                {
                    throw new BindingCompilationException(
                        $"Unexpected token '{expression.Substring(0, parser.Peek().StartPosition)} ---->{parser.Peek().Text}<---- {expression.Substring(parser.Peek().StartPosition + parser.Peek().Length)}'",
                        null, new TokenBase[] { parser.Peek() });
                }
                foreach (var n in node.EnumerateNodes())
                {
                    if (n.HasNodeErrors) throw new BindingCompilationException(string.Join(", ", n.NodeErrors), n);
                }

                var symbols = InitSymbols(dataContexts);
                symbols = options.AddTypes(symbols);

                var visitor = new ExpressionBuildingVisitor(symbols);
                visitor.Scope = symbols.Resolve(options.ScopeParameter);
                return visitor.Visit(node);
            }
            catch (Exception ex)
            {
                ex.ForInnerExceptions<BindingCompilationException>(bce =>
                {
                    if (bce.Expression == null) bce.Expression = expression;
                });
                throw;
            }
        }

        public static TypeRegistry InitSymbols(DataContextStack dataContext)
        {
            return AddTypeSymbols(TypeRegistry.Default.AddSymbols(GetParameters(dataContext).Select(d => new KeyValuePair<string, Expression>(d.Name, d))), dataContext);
        }

        public static TypeRegistry AddTypeSymbols(TypeRegistry reg, DataContextStack dataContext)
        {
            var namespaces = dataContext.Enumerable().Select(t => t.Namespace).Except(new[] { "System" }).Distinct();
            return reg.AddSymbols(new[]
            {
                // ViewModel is alias for current viewmodel type
                new KeyValuePair<string, Expression>("ViewModel", TypeRegistry.CreateStatic(dataContext.DataContextType)),
                // RootViewModel alias for root view model type
                new KeyValuePair<string, Expression>("RootViewModel", TypeRegistry.CreateStatic(dataContext.Enumerable().Last())),
            })
            // alias for any viewModel in hierarchy :
            .AddSymbols(dataContext.Enumerable()
                .Select((t, i) => new KeyValuePair<string, Expression>($"Parent{i}ViewModel", TypeRegistry.CreateStatic(t))))
            // import all viewModel namespaces
            .AddSymbols(namespaces.Select(ns => (Func<string, Expression>)(typeName => TypeRegistry.CreateStatic(ReflectionUtils.FindType(ns + "." + typeName)))));
        }

        public static IEnumerable<ParameterExpression> GetParameters(DataContextStack dataContext)
        {
            if (dataContext.RootControlType != null)
            {
                yield return Expression.Parameter(dataContext.RootControlType, "_control");
            }
            yield return Expression.Parameter(dataContext.DataContextType, "_this");
            yield return Expression.Parameter(typeof(BindingPageInfo), "_page");
            var index = 1;
            while (dataContext.Parent != null)
            {
                dataContext = dataContext.Parent;
                if (index == 1)
                {
                    yield return Expression.Parameter(dataContext.DataContextType, "_parent");
                }
                else
                {
                    yield return Expression.Parameter(dataContext.DataContextType, "_parent" + index);
                }
                index++;
            }
            yield return Expression.Parameter(dataContext.DataContextType, "_root");
        }
    }
}
