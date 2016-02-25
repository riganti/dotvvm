using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Exceptions;
using DotVVM.Framework.Runtime.ControlTree;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{
    public class CompileTimeBindingParser : IBindingParser
    {

        public Expression Parse(string expression, DataContextStack dataContexts, BindingParserOptions options)
        {
            try
            {
                var tokenizer = new Parser.Binding.Tokenizer.BindingTokenizer();
                tokenizer.Tokenize(new StringReader(expression));

                var parser = new Parser.Binding.Parser.BindingParser();
                parser.Tokens = tokenizer.Tokens;
                var node = parser.ReadExpression();
                if (!parser.OnEnd())
                    throw new BindingCompilationException(
                        $"unexpected token '{ expression.Substring(0, parser.Peek().StartPosition)} ---->{ parser.Peek().Text }<---- { expression.Substring(parser.Peek().StartPosition + parser.Peek().Length) }'",
                        null, new TokenBase[] { parser.Peek() });
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
            var type = dataContext.DataContextType;
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
            var index = 0;
            while (dataContext.Parent != null)
            {
                dataContext = dataContext.Parent;
                if (index == 0) yield return Expression.Parameter(dataContext.DataContextType, "_parent");
                yield return Expression.Parameter(dataContext.DataContextType, "_parent" + index);
                index++;
            }
            yield return Expression.Parameter(dataContext.DataContextType, "_root");
        }
    }
}
