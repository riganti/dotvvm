using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{
    public class TypeRegistry
    {
        ImmutableDictionary<string, Expression> registry;
        ImmutableList<Func<string, Expression>> resolvers;

        public TypeRegistry(ImmutableDictionary<string, Expression> registry, ImmutableList<Func<string, Expression>> resolvers)
        {
            this.registry = registry;
            this.resolvers = resolvers;
        }

        public Expression Resolve(string name, bool throwException = true)
        {
            Expression expr;
            if (registry.TryGetValue(name, out expr))
            {
                return expr;
            }
            expr = resolvers.Select(r => r(name)).FirstOrDefault(e => e != null);
            if (expr != null) return expr;
            if (throwException) throw new InvalidOperationException($"could not resolve identifier { name }");
            return null;
        }

        public TypeRegistry AddSymbols(IEnumerable<KeyValuePair<string, Expression>> symbols)
        {
            return new TypeRegistry(registry.AddRange(symbols), resolvers);
        }

        public TypeRegistry AddSymbols(IEnumerable<Func<string, Expression>> symbols)
        {
            return new TypeRegistry(registry, resolvers.InsertRange(0, symbols));
        }

        public static Expression CreateStatic(Type type)
        {
            return type == null ? null : Expression.Constant(null, type);
        }

        public static readonly TypeRegistry Default = new TypeRegistry(
            new[] {
            new KeyValuePair<string, Type>("object", typeof(Object)),
            new KeyValuePair<string, Type>("string", typeof(String)),
            new KeyValuePair<string, Type>("Object", typeof(Object)),
            new KeyValuePair<string, Type>("String", typeof(String))
                }.Select(k => new KeyValuePair<string, Expression>(k.Key, CreateStatic(k.Value))).ToImmutableDictionary(),
            new Func<string, Expression>[] {
                type => CreateStatic(ReflectionUtils.FindType(type)),
                type => CreateStatic(ReflectionUtils.FindType("System." + type))
                }.ToImmutableList());
    }
}
