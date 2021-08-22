#if !NET5_0_OR_GREATER

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices
{
    [DebuggerNonUserCode]
    [ExcludeFromCodeCoverage]
    internal static class IsExternalInit {}
}

#endif
