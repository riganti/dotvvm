using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotVVM.Analyzers.Serializability
{
    internal class SerializabilityAnalysisContext
    {
        public SemanticModel SemanticModel { get; set; }
        public INamedTypeSymbol? ViewModelSymbol { get; set; }
        public INamedTypeSymbol? BindAttributeSymbol { get; set; }
        public INamedTypeSymbol? JsonIgnoreAttributeSymbol { get; set; }
        private readonly SemanticModelAnalysisContext semanticAnalysisContext;
        private readonly Dictionary<ITypeSymbol, DiagnosticDescriptor?> symbolsLookup;

        public SerializabilityAnalysisContext(SemanticModelAnalysisContext context, INamedTypeSymbol? viewModel, INamedTypeSymbol? bindAttr, INamedTypeSymbol? jsonIgnoreAttr)
        {
            SemanticModel = context.SemanticModel;
            ViewModelSymbol = viewModel;
            BindAttributeSymbol = bindAttr;
            JsonIgnoreAttributeSymbol = jsonIgnoreAttr;
            semanticAnalysisContext = context;
#pragma warning disable RS1024 // Compare symbols correctly
            // This is a false positive: https://github.com/dotnet/roslyn-analyzers/issues/4568
            symbolsLookup = new Dictionary<ITypeSymbol, DiagnosticDescriptor?>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024
        }

        public void ReportDiagnostic(Diagnostic diagnostic)
            => semanticAnalysisContext.ReportDiagnostic(diagnostic);

        public void MarkAsVisited(ITypeSymbol symbol)
        {
            if (symbol.IsKnownSerializableType(SemanticModel.Compilation))
                return;

            symbolsLookup[symbol] = null;
        }

        public bool IsVisited(ITypeSymbol symbol)
            => symbolsLookup.ContainsKey(symbol) || symbol.IsKnownSerializableType(SemanticModel.Compilation);

        public void MarkAsSerializable(ITypeSymbol symbol)
            => symbolsLookup[symbol] = null;

        public void MarkAsNotSerializable(ITypeSymbol symbol, DiagnosticDescriptor descriptor)
            => symbolsLookup[symbol] = descriptor;

        public DiagnosticDescriptor? GetSerializabilityInfo(ITypeSymbol symbol)
        {
            if (!symbolsLookup.ContainsKey(symbol))
                return null;

            return symbolsLookup[symbol];
        }
    }
}
