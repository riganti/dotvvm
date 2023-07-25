using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Binding
{
    public class BindingExpressionBuilder : IBindingExpressionBuilder
    {
        private readonly CompiledAssemblyCache compiledAssemblyCache;
        private readonly ExtensionMethodsCache extensionMethodsCache;
        private MemberExpressionFactory? memberExpressionFactory;

        public BindingExpressionBuilder(CompiledAssemblyCache compiledAssemblyCache, ExtensionMethodsCache extensionMethodsCache)
        {
            this.compiledAssemblyCache = compiledAssemblyCache;
            this.extensionMethodsCache = extensionMethodsCache;
        }

        public Expression Parse(string expression, DataContextStack dataContexts, BindingParserOptions options, Type? expectedType = null, params KeyValuePair<string, Expression>[] additionalSymbols)
        {
            try
            {
                memberExpressionFactory = new MemberExpressionFactory(extensionMethodsCache, options.ImportNamespaces);

                var tokenizer = new BindingTokenizer();
                tokenizer.Tokenize(expression);

                var parser = new BindingParser();
                parser.Tokens = tokenizer.Tokens;
                var node = parser.ReadExpression();
                if (!parser.OnEnd())
                {
                    var bindingToken = parser.Peek().NotNull();
                    throw new BindingCompilationException(
                        $"Unexpected token '{expression.Substring(0, bindingToken.StartPosition)} ---->{bindingToken.Text}<---- {expression.Substring(bindingToken.EndPosition)}'",
                        null, new TokenBase[] { bindingToken });
                }
                foreach (var n in node.EnumerateNodes())
                {
                    if (n.HasNodeErrors) throw new BindingCompilationException(string.Join(", ", n.NodeErrors), n);
                }

                var symbols = InitSymbols(dataContexts);
                symbols = options.AddImportedTypes(symbols, compiledAssemblyCache);
                symbols = symbols.AddSymbols(options.ExtensionParameters.Select(p => CreateParameter(dataContexts, p.Identifier, p)));
                symbols = symbols.AddSymbols(additionalSymbols);

                var visitor = new ExpressionBuildingVisitor(symbols, memberExpressionFactory, expectedType);
                visitor.Scope = symbols.Resolve(options.ScopeParameter);
                var result = visitor.Visit(node);

                if (result is UnknownStaticClassIdentifierExpression resultError)
                    throw resultError.Error();
                return result;
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

        public TypeRegistry InitSymbols(DataContextStack dataContext)
        {
            return AddTypeSymbols(TypeRegistry.Default(compiledAssemblyCache)
                .AddSymbols(GetParameters(dataContext)
                .Select(d => new KeyValuePair<string, Expression>(d.Name!, d))), dataContext);
        }

        public TypeRegistry AddTypeSymbols(TypeRegistry reg, DataContextStack dataContext)
        {
            var namespaces = dataContext.Enumerable().Select(t => t?.Namespace).Except(new[] { "System", null }).Distinct();
            return reg.AddSymbols(new[] {
                    // ViewModel is alias for current viewmodel type
                    new KeyValuePair<string, Expression>("ViewModel", TypeRegistry.CreateStatic(dataContext.DataContextType)),
                    // RootViewModel alias for root view model type
                    new KeyValuePair<string, Expression>("RootViewModel", TypeRegistry.CreateStatic(dataContext.Enumerable().Last())),
                }.Concat(
                    // alias for any viewModel in hierarchy :
                    dataContext.Enumerable()
                    .Select((t, i) => new KeyValuePair<string, Expression>($"Parent{i}ViewModel", TypeRegistry.CreateStatic(t))))
            )
            // import all viewModel namespaces
            .AddSymbols(namespaces.Select(ns => (Func<string, Expression?>)(typeName => TypeRegistry.CreateStatic(compiledAssemblyCache.FindType(ns + "." + typeName)))));
        }

        public static IEnumerable<ParameterExpression> GetParameters(DataContextStack dataContext)
        {
            yield return CreateParameter(dataContext, "_this");

            foreach (var ext in dataContext.GetCurrentExtensionParameters())
            {
                yield return CreateParameter(
                    dataContext.EnumerableItems().ElementAt(ext.dataContextLevel),
                    ext.parameter.Identifier,
                    ext.parameter);
            }

            var index = 0;
            while (dataContext != null)
            {
                if (index == 1)
                    yield return CreateParameter(dataContext, "_parent");
                yield return CreateParameter(dataContext, "_parent" + index);
                if (dataContext.Parent == null)
                {
                    yield return CreateParameter(dataContext, "_root");
                }
                dataContext = dataContext.Parent!;
                index++;
            }
        }

        static ParameterExpression CreateParameter(DataContextStack stackItem, string name, BindingExtensionParameter? extensionParameter = null) =>
            Expression.Parameter(
                (extensionParameter == null
                    ? stackItem.DataContextType
                    : ResolvedTypeDescriptor.ToSystemType(extensionParameter.ParameterType))
                    ?? typeof(UnknownTypeSentinel)
                , name)
            .AddParameterAnnotation(new BindingParameterAnnotation(stackItem, extensionParameter));
    }

    public static class BindingExpressionBuilderExtension
    {
        public static Expression ParseWithLambdaConversion(this IBindingExpressionBuilder builder, string expression, DataContextStack dataContexts, BindingParserOptions options, Type expectedType, params KeyValuePair<string, Expression>[] additionalSymbols)
        {
            if (expectedType.IsDelegate(out var invokeMethod))
            {
                var resultType = invokeMethod.ReturnType;
                var delegateSymbols = invokeMethod
                                      .GetParameters()
                                      .Select((p, index) => new KeyValuePair<string, Expression>(
                                          p.Name.NotNull(),
                                          Expression.Parameter(p.ParameterType, p.Name)
                                              .AddParameterAnnotation(new BindingParameterAnnotation(
                                                  extensionParameter: new TypeConversion.MagicLambdaConversionExtensionParameter(index, p.Name, p.ParameterType)))
                                      ))
                                      .ToArray();
                return builder.Parse(expression, dataContexts, options, expectedType, additionalSymbols.Concat(delegateSymbols).ToArray());
            }
            else
                return builder.Parse(expression, dataContexts, options, expectedType, additionalSymbols);
        }
    }
}
