using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Parser;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{
    public class CompileTimeBindingParser : IBindingParser
    {

        public Expression Parse(string expression, DataContextStack dataContexts)
        {
            var tokenizer = new Parser.Binding.Tokenizer.BindingTokenizer();
            tokenizer.Tokenize(new StringReader(expression));

            var parser = new Parser.Binding.Parser.BindingParser();
            parser.Tokens = tokenizer.Tokens;
            var node = parser.ReadExpression();

            if (node.EnumerateNodes().Any(n => n.HasNodeErrors)) throw new BindingCompilationException($"parsing of expression { expression } failed", node);
            if (!parser.OnEnd()) throw new BindingCompilationException(
                $"unexpected token '{ expression.Substring(0, parser.Peek().StartPosition)} ---->{ parser.Peek().Text }<---- { expression.Substring(parser.Peek().StartPosition + parser.Peek().Length) }'", node);

            var visitor = new ExpressionBuldingVisitor(InitSymbols(dataContexts));
            visitor.Scope = Expression.Parameter(dataContexts.DataContextType, "_this");
            return visitor.Visit(node);
        }

        public static TypeRegistry InitSymbols(DataContextStack dataContext)
        {
            var type = dataContext.DataContextType;
            return TypeRegistry.Default.AddSymbols(GetParameters(dataContext).Select(d => new KeyValuePair<string, Expression>(d.Name, d)));
        }

        public static IEnumerable<ParameterExpression> GetParameters(DataContextStack dataContext)
        {
            if (dataContext.RootControlType != null)
            {
                yield return Expression.Parameter(dataContext.RootControlType, "_control");
            }
            yield return Expression.Parameter(dataContext.DataContextType, "_this");
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
