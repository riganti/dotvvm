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
        private const string allowStaticCommandAttributeMetadataName = "DotVVM.Framework.ViewModel.AllowStaticCommandAttribute";
        private const string staticCommandModelStateMetadataName = "DotVVM.Framework.Hosting.StaticCommandModelState";
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
                var staticCommandModelStateType = context.Compilation.GetTypeByMetadataName(staticCommandModelStateMetadataName);
                var expressionType = context.Compilation.GetTypeByMetadataName("System.Linq.Expressions.Expression`1");
                var stringType = context.Compilation.GetSpecialType(SpecialType.System_String);

                if (allowStaticCommandAttribute is null || staticCommandModelStateType == null)
                    return;

                if (context.Operation is IInvocationOperation invocation)
                {
                    var method = invocation.TargetMethod;
                    if (invocation.Instance?.Type != null &&
                        invocation.Arguments.Any() &&
                        SymbolEqualityComparer.Default.Equals(invocation.Instance.Type, staticCommandModelStateType) &&
                        invocation.TargetMethod.MetadataName == addArgumentErrorMetadataName &&
                        IsWithinAllowStaticCommandMethod(context, allowStaticCommandAttribute))
                    {
                        var staticCommand = (context.ContainingSymbol as IMethodSymbol)!;
                        var validExpression = true;
                        string? parameterName = null;

                        if (SymbolEqualityComparer.Default.Equals(invocation.Arguments[0].Parameter?.Type, stringType))
                        {
                            // Argument provided by string
                            parameterName = invocation.Arguments[0].Value switch {
                                ILiteralOperation local => local.ConstantValue.Value as string,
                                IConstantPatternOperation constant => constant.ConstantValue.Value as string,
                                INameOfOperation nameof => nameof.ConstantValue.Value as string,
                                _ => default,
                            };
                        }
                        else if (SymbolEqualityComparer.Default.Equals(invocation.Arguments[0].Parameter?.Type?.OriginalDefinition, expressionType))
                        {
                            // Argument (or its property) provided by lambda expression
                            (validExpression, parameterName) = GetParameterReferenceFromExpression(invocation);
                        }

                        // Check if the provided argument matched any parameter name of the method invoked by static command
                        if (!validExpression || (parameterName != null && !staticCommand.Parameters.Any(p => p.Name == parameterName)))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ReferenceOnlyArgumentsIncludedInStaticCommandInvocation,
                                    invocation.Arguments.First().Syntax.GetLocation(),
                                    (validExpression) ? parameterName : invocation.Arguments[0].Value.Syntax.ToFullString()));
                        }
                    }
                }
            }, OperationKind.Invocation);
        }

        private (bool ValidOperation, string? ParamName) GetParameterReferenceFromExpression(IInvocationOperation invocation)
        {
            IAnonymousFunctionOperation? lambda = null;
            var currentOperation = invocation.Arguments[0].Value;
            string? innerArgumentName = null;

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
                // Could not resolve argument
                return (true, null);
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
                    return (false, null);
                }
            }

            return (true, innerArgumentName);
        }

        private bool IsWithinAllowStaticCommandMethod(OperationAnalysisContext context, ISymbol allowStaticCommandType)
        {
            if (context.ContainingSymbol is not IMethodSymbol method)
                return false;

            return method.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, allowStaticCommandType)) != null;
        }
    }
}
