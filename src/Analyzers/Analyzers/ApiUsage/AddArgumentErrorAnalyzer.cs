using System.Collections.Immutable;
using System.Diagnostics;
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
        private static readonly LocalizableResourceString referenceTheSameParameterStaticCommandArgumentsTitle = new(nameof(Resources.ApiUsage_AddArgumentError_MismatchVariable_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString referenceTheSameParameterStaticCommandArgumentsMessage = new(nameof(Resources.ApiUsage_AddArgumentError_MismatchVariable_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString referenceTheSameParameterStaticCommandArgumentsDescription = new(nameof(Resources.ApiUsage_AddArgumentError_MismatchVariable_Description), Resources.ResourceManager, typeof(Resources));
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

        public static DiagnosticDescriptor ReferenceTheSameParameterInStaticCommandInvocation = new DiagnosticDescriptor(
            DotvvmDiagnosticIds.ReferenceTheSameParameterOnAddArgumentError,
            referenceTheSameParameterStaticCommandArgumentsTitle,
            referenceTheSameParameterStaticCommandArgumentsMessage,
            DiagnosticCategory.ApiUsage,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            referenceTheSameParameterStaticCommandArgumentsDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                ReferenceOnlyArgumentsIncludedInStaticCommandInvocation,
                ReferenceTheSameParameterInStaticCommandInvocation);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(context => {
                var allowStaticCommandAttribute = context.Compilation.GetTypeByMetadataName(allowStaticCommandAttributeMetadataName);
                var argumentModelStateType = context.Compilation.GetTypeByMetadataName(argumentModelStateMetadataName);
                var expressionType = context.Compilation.GetTypeByMetadataName("System.Linq.Expressions.Expression`1");

                if (allowStaticCommandAttribute is null || argumentModelStateType == null)
                    return;

                if (context.Operation is IInvocationOperation invocation)
                {
                    var method = invocation.TargetMethod;
                    if (invocation.Instance?.Type != null &&
                        invocation.Arguments.Any() &&
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

                        // Check if the provided argument matched any parameter name of the method invoked by static command
                        if (!staticCommand.Parameters.Any(p => p.Name == parameterName))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ReferenceOnlyArgumentsIncludedInStaticCommandInvocation,
                                    invocation.Arguments.First().Syntax.GetLocation(),
                                    parameterName));
                        }

                        // Check if there is no mismatch in arguments when providing property paths through expression
                        if (invocation.Arguments.Length == 3 &&
                            SymbolEqualityComparer.Default.Equals(invocation.Arguments[1]!.Parameter?.Type.OriginalDefinition, expressionType) &&
                            IsParameterReferenceIdentical(invocation, parameterName, out var innerArgumentName))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ReferenceTheSameParameterInStaticCommandInvocation,
                                    invocation.Syntax.GetLocation(),
                                    innerArgumentName,
                                    parameterName));
                        }
                    }
                }
            }, OperationKind.Invocation);
        }

        private bool IsParameterReferenceIdentical(IInvocationOperation invocation, string firstArgumentName, out string? innerArgumentName)
        {
            innerArgumentName = null;
            IAnonymousFunctionOperation? lambda = null;
            var currentOperation = invocation.Arguments[1].Value;

            while (currentOperation.Children.Any())
            {
                if (currentOperation is IAnonymousFunctionOperation anonymousFunctionOperation)
                {
                    lambda = anonymousFunctionOperation;
                    break;
                }
                currentOperation = currentOperation.Children.First();
            }

            if (lambda == null || lambda.Body.Operations.First() is not IReturnOperation returnOperation)
            {
                // Could not resolve argument => assume it is correct
                return true;
            }

            // Check if lambda body is valid
            currentOperation = returnOperation.ReturnedValue;
            while (currentOperation != null || innerArgumentName == null)
            {
                if (currentOperation is IPropertyReferenceOperation propertyReferenceOperation)
                {
                    currentOperation = propertyReferenceOperation.Instance;
                    continue;
                }
                else if (currentOperation is IParameterReferenceOperation parameterReferenceOperation)
                {
                    innerArgumentName = parameterReferenceOperation.Parameter.Name;
                    break;
                }
                else
                {
                    // Unexpected operation
                    Debugger.Break();
                    break;
                }
            }

            return innerArgumentName != null && innerArgumentName != firstArgumentName;
        }

        private bool IsWithinAllowStaticCommandMethod(OperationAnalysisContext context, ISymbol allowStaticCommandType)
        {
            if (context.ContainingSymbol is not IMethodSymbol method)
                return false;

            return method.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, allowStaticCommandType)) != null;
        }
    }
}
