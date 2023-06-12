using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
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
            DefaultExtensionParameters = defaultExtensionParameters ?? new BindingExtensionParameter[] {
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

        public DataContextStack CreateDataContext(Type[] contexts, BindingExtensionParameter[]? extensionParameters = null, Type? markupControl = null)
        {
            var ep = new List<BindingExtensionParameter>();
            if (extensionParameters is {})
                ep.AddRange(extensionParameters);
            if (markupControl is {})
                ep.Add(new CurrentMarkupControlExtensionParameter(new ResolvedTypeDescriptor(markupControl)));
            ep.AddRange(DefaultExtensionParameters);
            ep.AddRange(Configuration.Markup.DefaultExtensionParameters);

            var context = DataContextStack.Create(
                contexts.FirstOrDefault() ?? typeof(object),
                extensionParameters: ep.ToArray()
            );
            for (int i = 1; i < contexts.Length; i++)
            {
                context = DataContextStack.Create(contexts[i], context);
            }
            return context;
        }

        public Expression ParseBinding(string expression, DataContextStack context, Type? expectedType = null, NamespaceImport[]? imports = null)
        {
            expectedType ??= typeof(object);
            var parsedExpression = ExpressionBuilder.ParseWithLambdaConversion(expression, context, BindingParserOptions.Value.AddImports(imports), expectedType);
            return
                TypeConversion.MagicLambdaConversion(parsedExpression, expectedType) ??
                TypeConversion.ImplicitConversion(parsedExpression, expectedType, true, true)!;
        }

        public ParametrizedCode ValueBindingToParametrizedCode(string expression, Type[] contexts, Type? expectedType = null, NamespaceImport[]? imports = null, bool nullChecks = false, bool niceMode = true) =>
            ValueBindingToParametrizedCode(expression, CreateDataContext(contexts), expectedType, imports, nullChecks, niceMode);
        public ParametrizedCode ValueBindingToParametrizedCode(string expression, DataContextStack context, Type? expectedType = null, NamespaceImport[]? imports = null, bool nullChecks = false, bool niceMode = true)
        {
            expectedType ??= typeof(object);

            var expressionTree = ParseBinding(expression, context, expectedType, imports);
            var jsExpression = JavascriptTranslator.CompileToJavascript(expressionTree, context);
            return BindingPropertyResolvers.FormatJavascript(jsExpression, allowObservableResult: true, nullChecks: nullChecks, niceMode: niceMode);
        }

        public string ValueBindingToJs(string expression, Type[] contexts, Type? expectedType = null, NamespaceImport[]? imports = null, bool nullChecks = false, bool niceMode = true) =>
            JavascriptTranslator.FormatKnockoutScript(
                ValueBindingToParametrizedCode(expression, contexts, expectedType, imports, nullChecks, niceMode)
            );
        public string ValueBindingToJs(string expression, DataContextStack context, Type? expectedType = null, NamespaceImport[]? imports = null, bool nullChecks = false, bool niceMode = true) =>
            JavascriptTranslator.FormatKnockoutScript(
                ValueBindingToParametrizedCode(expression, context, expectedType, imports, nullChecks, niceMode)
            );

        public ValueBindingExpression<T> ValueBinding<T>(string expression, Type[] contexts)
        {
            return new ValueBindingExpression<T>(BindingService, new object[] {
                CreateDataContext(contexts),
                new OriginalStringBindingProperty(expression),
                BindingParserOptions.Value.AddImports(Configuration.Markup.ImportedNamespaces),
                new ExpectedTypeBindingProperty(typeof(T))
            });
        }

        public StaticCommandBindingExpression StaticCommand(string expression, Type[] contexts, Type? expectedType = null) =>
            StaticCommand(expression, CreateDataContext(contexts), expectedType);
        public StaticCommandBindingExpression StaticCommand(string expression, DataContextStack context, Type? expectedType = null)
        {
            expectedType ??= typeof(Command);
            return new StaticCommandBindingExpression(BindingService, new object[] {
                context,
                new OriginalStringBindingProperty(expression),
                BindingParserOptions.Value.AddImports(Configuration.Markup.ImportedNamespaces),
                new ExpectedTypeBindingProperty(expectedType)
            });
        }

        public static string GetStaticCommandJavascriptBody(StaticCommandBindingExpression binding, bool stripBoilerplate = true)
        {
            var expr = KnockoutHelper.GenerateClientPostBackExpression(
                "NonExistentProperty",
                binding,
                new Literal(),
                new PostbackScriptOptions(
                    allowPostbackHandlers: false,
                    returnValue: null,
                    commandArgs: CodeParameterAssignment.FromIdentifier("commandArguments")
                ));
            if (stripBoilerplate)
            {
                if (expr.StartsWith("dotvvm.applyPostbackHandlers(") && expr.EndsWith(",this,[],commandArguments)"))
                    expr = expr.Substring(29, expr.Length - 29 - 26);
                if (expr.StartsWith("async"))
                    expr = expr.Substring("async".Length).TrimStart();
                if (expr.StartsWith("(options)"))
                    expr = expr.Substring("(options)".Length).TrimStart();
                if (expr.StartsWith("=>"))
                    expr = expr.Substring("=>".Length).TrimStart();
            }
            return expr;
        }
    }
}
