using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressionEvaluator;
using ExpressionEvaluator.Parser;
using System.Reflection;
using System.Linq.Expressions;
using DotVVM.Framework.Parser;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class BindingParser : IBindingParser
    {
        private static MethodInfo ScopeCompileMethod =
            typeof(CompiledExpression)
            .GetMethods().First(m => m.Name == "ScopeCompile" && m.ContainsGenericParameters);

        public Expression Parse(string bindingExpressionText, Type contextType, Type[] parentContexts, Type controlType)
        {
            try
            {
                var parser = new AntlrParser(bindingExpressionText);
                parser.ExternalParameters = parser.ExternalParameters ?? new List<ParameterExpression>();
                parser.ExternalParameters.AddRange(GetParameters(contextType, parentContexts, controlType));
                parser.TypeRegistry = InitTypeRegistry(contextType, parentContexts);
                var scope = Expression.Parameter(contextType, Constants.ThisSpecialBindingProperty);
                return parser.Parse(scope);
            }
            catch (Exception exception)
            {
                throw new BidningParserExpception(contextType, bindingExpressionText, parentContexts, controlType, exception);
            }
        }

        private IEnumerable<ParameterExpression> GetParameters(Type contextType, Type[] parents, Type controlType = null)
        {
            if (controlType != null)
            {
                yield return Expression.Parameter(controlType, "_control");
            }
            yield return Expression.Parameter(contextType, "_this");
            if (parents.Length > 0)
            {
                yield return Expression.Parameter(parents.Last(), "_root");
                yield return Expression.Parameter(parents.First(), Constants.ParentSpecialBindingProperty);
                for (int i = 0; i < parents.Length; i++)
                {
                    yield return Expression.Parameter(parents[i], Constants.ParentSpecialBindingProperty + i);
                }
            }
            else
            {
                yield return Expression.Parameter(contextType, "_root");
            }
        }

        private TypeRegistry InitTypeRegistry(Type contextType, params Type[] parents)
        {
            var t = new TypeRegistry();
            t.RegisterType("ViewModel", contextType);
            t.RegisterType("RootViewModel", parents.LastOrDefault() ?? contextType);
            t.RegisterType("Enumerable", typeof(Enumerable));
            for (int i = parents.Length - 1; i >= 0; i--)
            {
                if (!t.ContainsKey(parents[i].Name))
                    t.RegisterType(parents[i].Name, parents[i]);
            }
            if (!t.ContainsKey(contextType.Name))
                t.RegisterType(contextType.Name, contextType);
            return t;
        }

        public Expression Parse(string value, DataContextStack context)
        {
            return Parse(value, context.DataContextType, context.Parents().ToArray(), context.RootControlType);
        }
    }
}
