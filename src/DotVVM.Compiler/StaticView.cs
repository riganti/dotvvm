using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DotVVM.Compiler
{
    public class StaticView
    {
        public StaticView(
            string viewPath,
            ImmutableArray<Report> reports = default,
            SyntaxTree? syntaxTree = default,
            ImmutableArray<MetadataReference> requiredReferences = default)
        {
            ViewPath = viewPath;
            Reports = reports.IsDefault ? ImmutableArray.Create<Report>() : reports;
            SyntaxTree = syntaxTree;
            RequiredReferences = requiredReferences.IsDefault
                ? ImmutableArray.Create<MetadataReference>()
                : requiredReferences;

        }

        public string ViewPath { get; }

        public ImmutableArray<Report> Reports { get; }

        public SyntaxTree? SyntaxTree { get; }

        public ImmutableArray<MetadataReference> RequiredReferences { get; }

        public StaticView WithReports(IEnumerable<Report> reports)
        {
            return new StaticView(ViewPath, reports.ToImmutableArray(), SyntaxTree, RequiredReferences);
        }

        public StaticView WithSyntaxTree(SyntaxTree syntaxTree)
        {
            return new StaticView(ViewPath, Reports, syntaxTree, RequiredReferences);
        }

        public StaticView WithRequiredReferences(IEnumerable<MetadataReference> requiredReferences)
        {
            return new StaticView(ViewPath, Reports, SyntaxTree, requiredReferences.ToImmutableArray());
        }
    }
}
