using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler
{
    public class StaticViewCompiler
    {
        public const string ObjectsClassName = "SerializedObjects";

        private readonly DotvvmConfiguration configuration;
        // controls and master pages justify the need for a cache
        private readonly ConcurrentDictionary<string, StaticView> viewCache
            = new ConcurrentDictionary<string, StaticView>();

        private bool canCompile;

        public StaticViewCompiler(DotvvmConfiguration configuration, bool canCompile = true)
        {
            this.configuration = configuration;
            this.canCompile = canCompile;
        }

        public StaticView GetView(string viewPath)
        {
            if (viewCache.ContainsKey(viewPath))
            {
                return viewCache[viewPath];
            }

            var view = CompileView(viewPath);
            // NB: in the meantime, the view could have been compiled on another thread
            return viewCache.GetOrAdd(viewPath, view);
        }

        public IEnumerable<StaticView> GetAllViews()
        {
            return configuration.RouteTable
                .Where(r => !string.IsNullOrWhiteSpace(r.VirtualPath))
                .Select(r => GetView(r.VirtualPath));
        }

        public (IEnumerable<StaticView>, EmitResult?) CompileAllViews(string assemblyName, Stream stream)
        {
            var views = GetAllViews().ToImmutableArray();
            if (views.Any(v => !v.Reports.IsEmpty))
            {
                // there are errors in the views
                stream.Close();
                return (views, null);
            }

            foreach(var view in views)
            {
                if (view.SyntaxTree is null)
                {
                    throw new ArgumentException(
                        $"The SyntaxTree of '{view.ViewPath}' is null although there are no errors.");
                }
            }

            var trees = views.Select(v => v.SyntaxTree);
            var references = views.SelectMany(v => v.RequiredReferences);
            var viewCompiler = configuration.ServiceProvider.GetRequiredService<IViewCompiler>();
            var compilation = viewCompiler.CreateCompilation(assemblyName);
            compilation = compilation.WithReferences(references).AddSyntaxTrees(trees);
            var result = compilation.Emit(stream);
            return (views, result);
        }

        private StaticView CompileView(string viewPath)
        {
            var fileLoader = configuration.ServiceProvider.GetRequiredService<IMarkupFileLoader>();
            var file = fileLoader.GetMarkup(configuration, viewPath);
            if (file is null)
            {
                throw new ArgumentException($"No view with path '{viewPath}' exists.");
            }

            // parse the document
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(file.ContentsReaderFactory());
            var parser = new DothtmlParser();
            var node = parser.Parse(tokenizer.Tokens);

            // analyze control types
            var controlTreeResolver = configuration.ServiceProvider.GetRequiredService<IControlTreeResolver>();
            var resolvedView = (ResolvedTreeRoot)controlTreeResolver.ResolveTree(node, viewPath);

            var view = new StaticView(viewPath);
            var reports = new List<Report>();

            try
            {
                var errorCheckingVisitor = new ErrorCheckingVisitor();
                resolvedView.Accept(errorCheckingVisitor);
            }
            catch(DotvvmCompilationException e)
            {
                reports.Add(new Report(e));
                // the error is too severe for compilation to continue 
                return view.WithReports(reports);
            }

            foreach (var n in node.EnumerateNodes())
            {
                if (n.HasNodeErrors)
                {
                    var line = n.Tokens.FirstOrDefault()?.LineNumber ?? -1;
                    var column = n.Tokens.FirstOrDefault()?.ColumnNumber ?? -1;

                    reports.AddRange(n.NodeErrors.Select(e => new Report(viewPath, line, column, e)));
                    // these errors are once again too severe
                    return view.WithReports(reports);
                }
            }

            var contextSpaceVisitor = new DataContextPropertyAssigningVisitor();
            resolvedView.Accept(contextSpaceVisitor);

            var styleVisitor = new StylingVisitor(configuration);
            resolvedView.Accept(styleVisitor);

            var usageValidator = configuration.ServiceProvider.GetRequiredService<IControlUsageValidator>();
            var validationVisitor = new ControlUsageValidationVisitor(usageValidator);
            resolvedView.Accept(validationVisitor);
            foreach(var error in validationVisitor.Errors)
            {
                var line = error.Nodes.FirstOrDefault()?.Tokens?.FirstOrDefault()?.LineNumber ?? -1;
                var column = error.Nodes.FirstOrDefault()?.Tokens?.FirstOrDefault().ColumnNumber ?? -1;

                reports.Add(new Report(viewPath, line, column, error.ErrorMessage));
            }

            if (reports.Any())
            {
                return view.WithReports(reports);
            }

            // no compilation errors beyond this point

            if (!canCompile)
            {
                return view;
            }

            var namespaceName = DefaultControlBuilderFactory.GetNamespaceFromFileName(
                file.FileName,
                file.LastWriteDateTimeUtc);
            var className = DefaultControlBuilderFactory.GetClassFromFileName(file.FileName) + "ControlBuilder";
            string fullClassName = namespaceName + "." + className;
            var refObjectSerializer = configuration.ServiceProvider.GetRequiredService<RefObjectSerializer>();
            var emitter = new CompileTimeCodeEmitter(refObjectSerializer, ObjectsClassName);
            var bindingCompiler = configuration.ServiceProvider.GetRequiredService<IBindingCompiler>();
            var compilingVisitor = new ViewCompilingVisitor(emitter, bindingCompiler, className);
            resolvedView.Accept(compilingVisitor);
            if (resolvedView.Directives.ContainsKey("masterPage"))
            {
                // make sure that the masterpage chain is already compiled
                _ = GetView(resolvedView.Directives["masterPage"].Single().Value);
            }

            var syntaxTree = emitter.BuildTree(namespaceName, className, viewPath).Single();
            var references = emitter.UsedAssemblies.Select(a => MetadataReference.CreateFromFile(a.Key.Location));
            return view.WithSyntaxTree(syntaxTree).WithRequiredReferences(references);
        }
    }
}
