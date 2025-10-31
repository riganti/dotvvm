using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace DotVVM.Analyzers.ApiUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DotvvmInterruptExceptionAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableResourceString title = new(nameof(Resources.ApiUsage_DoNotCatchDotvvmInterruptException_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString message = new(nameof(Resources.ApiUsage_DoNotCatchDotvvmInterruptException_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString description = new(nameof(Resources.ApiUsage_DoNotCatchDotvvmInterruptException_Description), Resources.ResourceManager, typeof(Resources));

        private const string dotvvmRequestContextMetadataName = "DotVVM.Framework.Hosting.IDotvvmRequestContext";
        private const string dotvvmInterruptExceptionMetadataName = "DotVVM.Framework.Hosting.DotvvmInterruptRequestExecutionException";

        public static DiagnosticDescriptor DoNotCatchDotvvmInterruptException = new DiagnosticDescriptor(
            DotvvmDiagnosticIds.DoNotCatchDotvvmInterruptRequestExecutionExceptionRuleId,
            title,
            message,
            DiagnosticCategory.ApiUsage,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(DoNotCatchDotvvmInterruptException);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(context =>
            {
                var dotvvmRequestContext = context.Compilation.GetTypeByMetadataName(dotvvmRequestContextMetadataName);
                if (dotvvmRequestContext is null)
                    return;

                var dotvvmInterruptException = context.Compilation.GetTypeByMetadataName(dotvvmInterruptExceptionMetadataName);
                if (dotvvmInterruptException is null)
                    return;

                if (context.Operation is IInvocationOperation invocation)
                {
                    var method = invocation.TargetMethod;
                    
                    // Check if this is a method on IDotvvmRequestContext that can throw DotvvmInterruptRequestExecutionException
                    if (!IsInterruptingMethod(method, dotvvmRequestContext))
                        return;

                    // Check if the invocation is within a try-catch block
                    var tryOperation = GetEnclosingTryOperation(invocation);
                    if (tryOperation is null)
                        return;

                    // Check if DotvvmInterruptRequestExecutionException is properly rethrown
                    if (HasProperRethrow(tryOperation, dotvvmInterruptException))
                        return;

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DoNotCatchDotvvmInterruptException,
                            invocation.Syntax.GetLocation(),
                            method.Name));
                }
            }, OperationKind.Invocation);
        }

        private static bool IsInterruptingMethod(IMethodSymbol method, INamedTypeSymbol dotvvmRequestContext)
        {
            // Check if this is an extension method on IDotvvmRequestContext
            if (!method.IsExtensionMethod)
                return false;

            if (method.Parameters.Length == 0)
                return false;

            var firstParam = method.Parameters[0];
            if (!SymbolEqualityComparer.Default.Equals(firstParam.Type, dotvvmRequestContext))
                return false;

            // Check if the method name matches one of the known interrupting methods
            // These methods throw DotvvmInterruptRequestExecutionException after setting the response
            var methodName = method.Name;
            return methodName.StartsWith("RedirectTo") || 
                   methodName.StartsWith("ReturnFile");
        }

        private static ITryOperation? GetEnclosingTryOperation(IOperation operation)
        {
            var current = operation.Parent;
            while (current != null)
            {
                if (current is ITryOperation tryOperation)
                    return tryOperation;
                current = current.Parent;
            }
            return null;
        }

        private static bool HasProperRethrow(ITryOperation tryOperation, INamedTypeSymbol dotvvmInterruptException)
        {
            // Iterate through catch clauses in order (as they are evaluated in order at runtime)
            foreach (var catchClause in tryOperation.Catches)
            {
                var caughtType = catchClause.ExceptionType;
                if (caughtType == null)
                    continue;

                // Check if this catch could catch the interrupt exception
                if (IsCatchingInterruptException(caughtType, dotvvmInterruptException))
                {
                    // If this catch specifically catches DotvvmInterruptRequestExecutionException and rethrows, that's OK
                    if (SymbolEqualityComparer.Default.Equals(caughtType, dotvvmInterruptException) &&
                        IsJustRethrow(catchClause.Handler))
                    {
                        // This catch handles the interrupt exception properly by rethrowing it
                        // The exception won't reach subsequent catch blocks, so we're safe
                        return true;
                    }
                    
                    // This catch could catch the interrupt exception but doesn't handle it properly
                    return false;
                }
            }

            // No catch catches the interrupt exception
            return true;
        }

        private static bool IsCatchingInterruptException(ITypeSymbol caughtType, INamedTypeSymbol dotvvmInterruptException)
        {
            // Check if caughtType is DotvvmInterruptRequestExecutionException or a base type
            var current = dotvvmInterruptException as ITypeSymbol;
            while (current != null)
            {
                if (SymbolEqualityComparer.Default.Equals(caughtType, current))
                    return true;
                current = current.BaseType;
            }
            return false;
        }

        private static bool IsJustRethrow(IBlockOperation? block)
        {
            if (block == null)
                return false;

            var operations = block.Operations;
            if (operations.Length != 1)
                return false;

            return operations[0] is IThrowOperation throwOp && throwOp.Exception == null;
        }
    }
}
