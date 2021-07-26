using System;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Testing
{
    public class BindingTestHelper
    {
        public BindingTestHelper(
            DotvvmConfiguration? configuration = null,
            BindingCompilationService? bindingService = null,
            BindingExtensionParameter[]? defaultExtensionParameters = null)
        {
            Configuration = configuration ?? DotvvmTestHelper.DefaultConfig;
            BindingService = bindingService ?? Configuration.ServiceProvider.GetRequiredService<BindingCompilationService>();
            JavascriptTranslator = Configuration.ServiceProvider.GetRequiredService<JavascriptTranslator>();
            ExpressionBuilder = Configuration.ServiceProvider.GetRequiredService<IBindingExpressionBuilder>();
            DefaultExtensionParameters = defaultExtensionParameters ?? new BindingExtensionParameter[]{
                new CurrentCollectionIndexExtensionParameter(),
                new BindingCollectionInfoExtensionParameter("_collection"),
                new BindingPageInfoExtensionParameter(),
                new BindingApiExtensionParameter(),
            };
        }

        public DotvvmConfiguration Configuration { get; }
        public BindingCompilationService BindingService { get; }
        public BindingExtensionParameter[] DefaultExtensionParameters { get; }
        public JavascriptTranslator JavascriptTranslator { get; }
        public IBindingExpressionBuilder ExpressionBuilder { get; }

        public DataContextStack CreateDataContext(Type[] contexts)
        {
            var context = DataContextStack.Create(
                contexts.FirstOrDefault() ?? typeof(object),
                extensionParameters: DefaultExtensionParameters.Concat(Configuration.Markup.DefaultExtensionParameters).ToArray()
            );
            for (int i = 1; i < contexts.Length; i++)
            {
                context = DataContextStack.Create(contexts[i], context);
            }
            return context;
        }

        public Expression ParseBinding(string expression, DataContextStack context, Type? expectedType = null, NamespaceImport[]? imports = null)
        {
            var parsedExpression = ExpressionBuilder.ParseWithLambdaConversion(expression, context, BindingParserOptions.Value.AddImports(imports), expectedType);
            return
                TypeConversion.MagicLambdaConversion(parsedExpression, expectedType) ??
                TypeConversion.ImplicitConversion(parsedExpression, expectedType, true, true);
        }

        public ParametrizedCode ValueBindingToParametrizedCode(string expression, Type[] contexts, Type? expectedType = null, NamespaceImport[]? imports = null, bool nullChecks = false, bool niceMode = true)
        {
            expectedType ??= typeof(object);

            var context = CreateDataContext(contexts);
            var expressionTree = ParseBinding(expression, context, expectedType, imports);
            var jsExpression = JavascriptTranslator.CompileToJavascript(expressionTree, context);
            return BindingPropertyResolvers.FormatJavascript(jsExpression, allowObservableResult: true, nullChecks: nullChecks, niceMode: niceMode);
        }

        public string ValueBindingToJs(string expression, Type[] contexts, Type? expectedType = null, NamespaceImport[]? imports = null, bool nullChecks = false, bool niceMode = true)
        {
            return JavascriptTranslator.FormatKnockoutScript(
                ValueBindingToParametrizedCode(expression, contexts, expectedType, imports, nullChecks, niceMode)
            );
        }

        public ValueBindingExpression<T> ValueBinding<T>(string expression, Type[] contexts)
        {
            return new ValueBindingExpression<T>(BindingService, new object[] {
                CreateDataContext(contexts),
                new OriginalStringBindingProperty(expression),
                new BindingParserOptions(typeof(ValueBindingExpression)).AddImports(Configuration.Markup.ImportedNamespaces),
                new ExpectedTypeBindingProperty(typeof(T))
            });
        }
    }
}
