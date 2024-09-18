using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DotVVM.Analyzers
{
    internal static class RoslynTreeHelper
    {
        // from https://github.com/Evangelink/roslyn-analyzers/blob/48424637e03e48bbbd8e02862c940e7eb5436817/src/Utilities/Compiler/Extensions/IOperationExtensions.cs
        private static readonly ImmutableArray<OperationKind> s_LambdaAndLocalFunctionKinds =
            ImmutableArray.Create(OperationKind.AnonymousFunction, OperationKind.LocalFunction);

        /// <summary>
        /// Gets the first ancestor of this operation with:
        ///  1. Any OperationKind from the specified <paramref name="ancestorKinds"/>.
        ///  2. If <paramref name="predicate"/> is non-null, it succeeds for the ancestor.
        /// Returns null if there is no such ancestor.
        /// </summary>
        public static IOperation? GetAncestor(this IOperation root, ImmutableArray<OperationKind> ancestorKinds, Func<IOperation, bool>? predicate = null)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            var ancestor = root;
            do
            {
                ancestor = ancestor.Parent;
            } while (ancestor != null && !ancestorKinds.Contains(ancestor.Kind));

            if (ancestor != null)
            {
                if (predicate != null && !predicate(ancestor))
                {
                    return GetAncestor(ancestor, ancestorKinds, predicate);
                }
                return ancestor;
            }
            else
            {
                return default;
            }
        }

        public static bool IsWithinExpressionTree(this IOperation operation, INamedTypeSymbol? linqExpressionTreeType)
            => linqExpressionTreeType != null
                && operation.GetAncestor(s_LambdaAndLocalFunctionKinds)?.Parent?.Type?.OriginalDefinition is { } lambdaType
                && SymbolEqualityComparer.Default.Equals(linqExpressionTreeType, lambdaType);
    }
}
