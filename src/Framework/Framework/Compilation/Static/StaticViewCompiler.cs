
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ViewCompiler;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Framework.Compilation.Static
{
    internal static class StaticViewCompiler
    {
        public static ImmutableArray<DotvvmCompilationDiagnostic> CompileAll(
            Assembly dotvvmProjectAssembly,
            string dotvvmProjectDir,
            IReadOnlyList<string>? filesToCheck = null)
        {
            var configuration = ConfigurationInitializer.GetConfiguration(dotvvmProjectAssembly, dotvvmProjectDir);
            var diagnostics = ImmutableArray.CreateBuilder<DotvvmCompilationDiagnostic>();
            var compiledPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var markupControls = configuration.Markup.Controls.Select(c => c.Src)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToImmutableArray();
            foreach (var markupControl in markupControls)
            {
                compiledPaths.Add(markupControl!);
                diagnostics.AddRange(CompileNoThrow(configuration, markupControl!, out _));
            }

            var views = configuration.RouteTable.Select(r => r.VirtualPath).WhereNotNull().ToImmutableArray();
            var discoveredMasterPages = new Queue<string>();
            foreach(var view in views)
            {
                compiledPaths.Add(view);
                diagnostics.AddRange(CompileNoThrow(configuration, view, out var masterPage));
                if (masterPage is not null && compiledPaths.Add(masterPage))
                    discoveredMasterPages.Enqueue(masterPage);
            }

            // Discover master pages transitively (a master page may itself use another master page).
            while (discoveredMasterPages.Count > 0)
            {
                var masterPagePath = discoveredMasterPages.Dequeue();
                diagnostics.AddRange(CompileNoThrow(configuration, masterPagePath, out var nestedMaster));
                if (nestedMaster is not null && compiledPaths.Add(nestedMaster))
                    discoveredMasterPages.Enqueue(nestedMaster);
            }

            var allDiagnostics = diagnostics.Distinct().ToImmutableArray();

            if (filesToCheck is { Count: > 0 })
            {
                var filterSet = new HashSet<string>(filesToCheck, StringComparer.OrdinalIgnoreCase);
                return allDiagnostics
                    .Where(d => d.Location.FileName is not null && filterSet.Contains(d.Location.FileName))
                    .ToImmutableArray();
            }

            return allDiagnostics;
        }

        private static ImmutableArray<DotvvmCompilationDiagnostic> CompileNoThrow(
            DotvvmConfiguration configuration,
            string viewPath,
            out string? masterPage)
        {
            masterPage = null;
            var fileLoader = configuration.ServiceProvider.GetRequiredService<IMarkupFileLoader>();
            var file = fileLoader.GetMarkup(configuration, viewPath);
            if (file is null)
            {
                return ImmutableArray.Create<DotvvmCompilationDiagnostic>();
            }

            var sourceCode = file.ReadContent();

            try
            {
                var compiler = configuration.ServiceProvider.GetRequiredService<IViewCompiler>();
                var (descriptor, builderFactory) = compiler.CompileView(
                    sourceCode: sourceCode,
                    fileName: viewPath);
                _ = builderFactory();
                masterPage = descriptor.MasterPage?.FileName;
                // TODO: get warnings from compilation tracer
                return ImmutableArray.Create<DotvvmCompilationDiagnostic>();
            }
            catch(DotvvmCompilationException e)
            {
                return e.AllDiagnostics
                    .Select(d => d.Location.FileName is null
                        ? d with { Location = d.Location with { FileName = viewPath } }
                        : d)
                    .ToImmutableArray();
            }
        }
    }
}
