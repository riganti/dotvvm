using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace DotVVM.Analyzers.ApiUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddArgumentErrorAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableResourceString referenceOnlyStaticCommandArgumentsTitle = new(nameof(Resources.ApiUsage_AddArgumentError_InvalidVariable_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString referenceOnlyStaticCommandArgumentsMessage = new(nameof(Resources.ApiUsage_AddArgumentError_InvalidVariable_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString referenceOnlyStaticCommandArgumentsDescription = new(nameof(Resources.ApiUsage_AddArgumentError_InvalidVariable_Description), Resources.ResourceManager, typeof(Resources));
        private const string allowStaticCommandAttributeMetadataName = "DotVVM.Framework.ViewModel.AllowStaticCommandAttribute";
        private const string argumentModelStateMetadataName = "DotVVM.Framework.Hosting.ArgumentModelState";
        private const string addArgumentErrorMetadataName = "AddArgumentError";

        public static DiagnosticDescriptor ReferenceOnlyArgumentsIncludedInStaticCommandInvocation = new DiagnosticDescriptor(
            DotvvmDiagnosticIds.ReferenceOnlyStaticCommandArgumentsOnValidationError,
            referenceOnlyStaticCommandArgumentsTitle,
            referenceOnlyStaticCommandArgumentsMessage,
            DiagnosticCategory.ApiUsage,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            referenceOnlyStaticCommandArgumentsDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(ReferenceOnlyArgumentsIncludedInStaticCommandInvocation);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(context => {
                var allowStaticCommandAttribute = context.Compilation.GetTypeByMetadataName(allowStaticCommandAttributeMetadataName);
                var argumentModelStateType = context.Compilation.GetTypeByMetadataName(argumentModelStateMetadataName);

                if (allowStaticCommandAttribute is null || argumentModelStateType == null)
                    return;

                if (context.Operation is IInvocationOperation invocation)
                {
                    var method = invocation.TargetMethod;
                    if (invocation.Instance?.Type != null &&
                        SymbolEqualityComparer.Default.Equals(invocation.Instance.Type, argumentModelStateType) &&
                        invocation.TargetMethod.MetadataName == addArgumentErrorMetadataName &&
                        IsWithinAllowStaticCommandMethod(context, allowStaticCommandAttribute))
                    {
                        var staticCommand = (context.ContainingSymbol as IMethodSymbol)!;
                        var parameterName = invocation.Arguments[0].Value switch
                        {
                            ILocalReferenceOperation local => local.Local.Name,
                            ILiteralOperation local => local.ConstantValue.Value as string,
                            IConstantPatternOperation constant => constant.ConstantValue.Value as string,
                            INameOfOperation nameof => nameof.Argument.ConstantValue.Value as string,
                            _ => default,
                        };

                        if (parameterName == null)
                        {
                            // Argument can not be evaluated during compile-time
                            // Let's assume user knows what they are doing
                            return;
                        }

                        if (!staticCommand.Parameters.Any(p => p.Name == parameterName))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ReferenceOnlyArgumentsIncludedInStaticCommandInvocation,
                                    invocation.Arguments.First().Syntax.GetLocation(),
                                    parameterName));
                        }
                    }
                }
            }, OperationKind.Invocation);
        }

        private bool IsWithinAllowStaticCommandMethod(OperationAnalysisContext context, ISymbol allowStaticCommandType)
        {
            if (context.ContainingSymbol is not IMethodSymbol method)
                return false;

            return method.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, allowStaticCommandType)) != null;
        }
    }
}
