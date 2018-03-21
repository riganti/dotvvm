using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Symbols.Registries
{
    public struct MemberInfo
    {
        public bool WasCompiled { get; set; }
        public ISymbol Symbol { get; set; }

        public MemberInfo(ISymbol symbol, bool wasCompiled = false)
        {
            WasCompiled = wasCompiled;
            Symbol = symbol;
        }
    }
}