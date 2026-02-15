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

                    // Check if the invocation is within a try-catch block and validate all nested try blocks
                    if (!HasProperExceptionHandling(invocation, dotvvmInterruptException))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DoNotCatchDotvvmInterruptException,
                                invocation.Syntax.GetLocation(),
                                method.Name));
                    }
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

        /// <summary>
        /// Checks if the invocation has proper exception handling for all enclosing try blocks
        /// </summary>
        private static bool HasProperExceptionHandling(IOperation operation, INamedTypeSymbol dotvvmInterruptException)
        {
            var current = operation.Parent;
            while (current != null)
            {
                if (current is ITryOperation tryOperation)
                {
                    if (!HasProperRethrow(tryOperation, dotvvmInterruptException))
                        return false;
                }
                current = current.Parent;
            }
            
            return true;
        }

        private static bool HasProperRethrow(ITryOperation tryOperation, INamedTypeSymbol dotvvmInterruptException)
        {
            foreach (var catchClause in tryOperation.Catches)
            {
                var caughtType = catchClause.ExceptionType;

                // Check if the "when" clause contains the interrupt exception
                // We cannot evaluate the condition, but if it mentions the interrupt exception type, we assume it's being excluded (e.g., "when (ex is not DotvvmInterruptRequestExecutionException)")
                if (HasWhenClauseExcludingInterruptException(catchClause, dotvvmInterruptException))
                {
                    return true;
                }

                // A bare catch { } (no exception type) or catch (Exception) { } catches all exceptions
                // If it contains a rethrow anywhere, we consider it safe
                if (caughtType.Name == "Object" || caughtType.Name == "Exception")
                {
                    if (ContainsRethrow(catchClause.Handler))
                    {
                        return true;
                    }

                    return false;
                }

                // Catch of the interrupt exception
                if (IsCatchingInterruptException(caughtType, dotvvmInterruptException))
                {
                    if (ContainsRethrow(catchClause.Handler))
                    {
                        return true;
                    }
                    
                    return false;
                }
            }

            // No catch catches the interrupt exception
            return true;
        }

        private static bool HasWhenClauseExcludingInterruptException(ICatchClauseOperation catchClause, INamedTypeSymbol dotvvmInterruptException)
        {
            if (catchClause.Filter == null)
                return false;

            var filterSyntax = catchClause.Filter.Syntax.ToString();
            return filterSyntax.Contains(dotvvmInterruptException.Name);
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

        private static bool ContainsRethrow(IOperation? handler)
        {
            if (handler == null)
                return false;

            return ContainsRethrowRecursive(handler);
        }

        private static bool ContainsRethrowRecursive(IOperation operation)
        {
            if (operation is IThrowOperation throwOp && throwOp.Exception == null)
                return true;

            foreach (var child in operation.Children)
            {
                if (ContainsRethrowRecursive(child))
                    return true;
            }

            return false;
        }
    }
}
