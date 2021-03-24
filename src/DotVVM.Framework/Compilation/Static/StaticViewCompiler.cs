#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Framework.Compilation.Static
{
    internal static class StaticViewCompiler
    {
        public static ImmutableArray<CompilationReport> CompileAll(
            Assembly dotvvmProjectAssembly,
            string dotvvmProjectDir)
        {
            var configuration = ConfigurationInitializer.GetConfiguration(dotvvmProjectAssembly, dotvvmProjectDir);
            var reportsBuilder = ImmutableArray.CreateBuilder<CompilationReport>();

            var markupControls = configuration.Markup.Controls.Select(c => c.Src)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToImmutableArray();
            foreach (var markupControl in markupControls)
            {
                reportsBuilder.AddRange(CompileNoThrow(configuration, markupControl!));
            }

            var views = configuration.RouteTable.Select(r => r.VirtualPath).ToImmutableArray();
            foreach(var view in views)
            {
                reportsBuilder.AddRange(CompileNoThrow(configuration, view));
            }

            return reportsBuilder.Distinct().ToImmutableArray();
        }

        private static ImmutableArray<CompilationReport> CompileNoThrow(
            DotvvmConfiguration configuration,
            string viewPath)
        {
            var fileLoader = configuration.ServiceProvider.GetRequiredService<IMarkupFileLoader>();
            var file = fileLoader.GetMarkup(configuration, viewPath);
            if (file is null)
            {
                return ImmutableArray.Create<CompilationReport>();
            }

            var namespaceName = DefaultControlBuilderFactory.GetNamespaceFromFileName(
                file.FileName,
                file.LastWriteDateTimeUtc);
            var className = DefaultControlBuilderFactory.GetClassFromFileName(file.FileName) + "ControlBuilder";
            var fullClassName = namespaceName + "." + className;
            var sourceCode = file.ContentsReaderFactory();

            try
            {
                var compiler = configuration.ServiceProvider.GetRequiredService<IViewCompiler>();
                var (_, builderFactory) = compiler.CompileView(
                    sourceCode: sourceCode,
                    fileName: viewPath,
                    assemblyName: $"{fullClassName}.Compiled",
                    namespaceName: namespaceName,
                    className: className);
                _ = builderFactory();
                // TODO: Reporting compiler errors solely through exceptions is dumb. I have no way of getting all of
                //       the parser errors at once because they're reported through exceptions one at a time. We need
                //       to rewrite DefaultViewCompiler and its interface if the static linter/compiler is to be useful.
                return ImmutableArray.Create<CompilationReport>();
            }
            catch(DotvvmCompilationException e)
            {
                return ImmutableArray.Create(new CompilationReport(viewPath, e));
            }
        }
    }
}
