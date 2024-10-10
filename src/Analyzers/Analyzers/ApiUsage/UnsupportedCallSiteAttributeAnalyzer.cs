﻿using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace DotVVM.Analyzers.ApiUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnsupportedCallSiteAttributeAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableResourceString unsupportedCallSiteTitle = new(nameof(Resources.ApiUsage_UnsupportedCallSite_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString unsupportedCallSiteMessage = new(nameof(Resources.ApiUsage_UnsupportedCallSite_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString unsupportedCallSiteDescription = new(nameof(Resources.ApiUsage_UnsupportedCallSite_Description), Resources.ResourceManager, typeof(Resources));
        private const string unsupportedCallSiteAttributeMetadataName = "DotVVM.Framework.CodeAnalysis.UnsupportedCallSiteAttribute";
        private const string linqExpressionsExpression1MetadataName = "System.Linq.Expressions.Expression`1";
        private const int callSiteTypeServerUnderlyingValue = 0;

        public static DiagnosticDescriptor DoNotInvokeMethodFromUnsupportedCallSite = new DiagnosticDescriptor(
            DotvvmDiagnosticIds.DoNotInvokeMethodFromUnsupportedCallSiteRuleId,
            unsupportedCallSiteTitle,
            unsupportedCallSiteMessage,
            DiagnosticCategory.ApiUsage,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            unsupportedCallSiteDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(DoNotInvokeMethodFromUnsupportedCallSite);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(context =>
            {
                var unsupportedCallSiteAttribute = context.Compilation.GetTypeByMetadataName(unsupportedCallSiteAttributeMetadataName);
                if (unsupportedCallSiteAttribute is null)
                    return;

                if (context.Operation is IInvocationOperation invocation)
                {
                    var method = invocation.TargetMethod;
                    var attribute = method.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, unsupportedCallSiteAttribute));
                    if (attribute is null || !attribute.ConstructorArguments.Any())
                        return;

                    if (attribute.ConstructorArguments.First().Value is not int callSiteType || callSiteTypeServerUnderlyingValue != callSiteType)
                        return;

                    if (context.Operation.IsWithinExpressionTree(context.Compilation.GetTypeByMetadataName(linqExpressionsExpression1MetadataName)))
                        // supress in Linq.Expression trees, such as in ValueOrBinding.Select
                        return;

                    var reason = (string?)attribute.ConstructorArguments.Skip(1).First().Value;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DoNotInvokeMethodFromUnsupportedCallSite,
                            invocation.Syntax.GetLocation(),
                            invocation.TargetMethod.Name,
                            (reason != null) ? $"due to: \"{reason}\"" : string.Empty));
                }
            }, OperationKind.Invocation);
        }
    }
}
