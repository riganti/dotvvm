#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace DotVVM.Framework.Compilation.Static
{
    internal class StaticView
    {
        public StaticView(
            string viewPath,
            ImmutableArray<CompilationReport> reports = default,
            SyntaxTree? syntaxTree = default,
            ImmutableArray<MetadataReference> requiredReferences = default,
            Assembly? assembly = default,
            Type? viewType = default,
            Type? dataContextType = default)
        {
            ViewPath = viewPath;
            Reports = reports.IsDefault ? ImmutableArray.Create<CompilationReport>() : reports;
            SyntaxTree = syntaxTree;
            RequiredReferences = requiredReferences.IsDefault
                ? ImmutableArray.Create<MetadataReference>()
                : requiredReferences;
            Assembly = assembly;
            ViewType = viewType;
            DataContextType = dataContextType;
        }

        public string ViewPath { get; }

        public ImmutableArray<CompilationReport> Reports { get; }

        public SyntaxTree? SyntaxTree { get; }

        public ImmutableArray<MetadataReference> RequiredReferences { get; }

        public Assembly? Assembly { get; }

        public Type? ViewType { get; }

        public Type? DataContextType { get; }

        public StaticView WithReports(IEnumerable<CompilationReport> reports)
        {
            return new StaticView(
                ViewPath,
                reports.ToImmutableArray(),
                SyntaxTree,
                RequiredReferences,
                Assembly,
                ViewType,
                DataContextType);
        }

        public StaticView WithSyntaxTree(SyntaxTree syntaxTree)
        {
            return new StaticView(
                ViewPath,
                Reports,
                syntaxTree,
                RequiredReferences,
                Assembly,
                ViewType,
                DataContextType);
        }

        public StaticView WithRequiredReferences(IEnumerable<MetadataReference> requiredReferences)
        {
            return new StaticView(
                ViewPath,
                Reports,
                SyntaxTree,
                requiredReferences.ToImmutableArray(),
                Assembly,
                ViewType,
                DataContextType);
        }

        public StaticView WithAssembly(Assembly assembly)
        {
            return new StaticView(
                ViewPath,
                Reports,
                SyntaxTree,
                RequiredReferences,
                Assembly,
                ViewType,
                DataContextType);
        }

        public StaticView WithViewType(Type viewType)
        {
            return new StaticView(
                ViewPath,
                Reports,
                SyntaxTree,
                RequiredReferences,
                Assembly,
                viewType,
                DataContextType);
        }

        public StaticView WithDataContextType(Type dataContextType)
        {
            return new StaticView(
                ViewPath,
                Reports,
                SyntaxTree,
                RequiredReferences,
                Assembly,
                ViewType,
                dataContextType);
        }
    }
}
