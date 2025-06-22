using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DotVVM.Framework.Utils
{
    internal static class ThrowHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(argument);
#else
            if (argument is null)
            {
                ThrowArgumentNullException(paramName);
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        private static void ThrowArgumentNullException(string? paramName)
        {
            throw new ArgumentNullException(paramName);
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ObjectDisposed(bool isDisposed, object @this)
        {
#if NET6_0_OR_GREATER
            ObjectDisposedException.ThrowIf(isDisposed, @this);
#else
            if (argument is null)
            {
                ThrowArgumentNullException(paramName);
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        private static void ThrowObjectDisposedException(object @this)
        {
            throw new ObjectDisposedException(@this?.GetType().FullName);
#endif
        }
    }
}
