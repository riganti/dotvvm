using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation
{
    public class DotvvmViewCompilationService : IDotvvmViewCompilationService
    {
        private IControlBuilderFactory controlBuilderFactory;
        private readonly DotvvmConfiguration dotvvmConfiguration;

        public DotvvmViewCompilationService(DotvvmConfiguration dotvvmConfiguration, IControlBuilderFactory controlBuilderFactory)
        {
            this.dotvvmConfiguration = dotvvmConfiguration;
            this.controlBuilderFactory = controlBuilderFactory;
            masterPages = new Lazy<ConcurrentDictionary<string, DotHtmlFileInfo>>(InitMasterPagesCollection);
            controls = new Lazy<ImmutableArray<DotHtmlFileInfo>>(InitControls);
            routes = new Lazy<ImmutableArray<DotHtmlFileInfo>>(InitRoutes);
        }

        public ImmutableArray<DotHtmlFileInfo> GetFilesWithFailedCompilation()
        {
            return masterPages.Value.Values.Concat(controls.Value).Concat(routes.Value)
                .Where(t => t.Status == CompilationState.CompilationFailed).ToImmutableArray();
        }

        private readonly Lazy<ConcurrentDictionary<string, DotHtmlFileInfo>> masterPages;
        private ConcurrentDictionary<string, DotHtmlFileInfo> InitMasterPagesCollection()
        {
            return new ConcurrentDictionary<string, DotHtmlFileInfo>();
        }
        public ImmutableArray<DotHtmlFileInfo> GetMasterPages()
        {
            return masterPages.Value.Values.ToImmutableArray();
        }

        private readonly Lazy<ImmutableArray<DotHtmlFileInfo>> controls;
        private ImmutableArray<DotHtmlFileInfo> InitControls()
        {
            return
                dotvvmConfiguration.Markup.Controls.Where(s => !string.IsNullOrWhiteSpace(s.Src))
                    .Select(s => new DotHtmlFileInfo(s.Src!, tagPrefix: s.TagPrefix, tagName: s.TagName,
                        nameSpace: s.Namespace, assembly: s.Assembly)).ToImmutableArray();
        }

        public ImmutableArray<DotHtmlFileInfo> GetControls()
        {
            return controls.Value;
        }

        private readonly Lazy<ImmutableArray<DotHtmlFileInfo>> routes;
        private ImmutableArray<DotHtmlFileInfo> InitRoutes()
        {
            return dotvvmConfiguration.RouteTable.Select(r =>
                new DotHtmlFileInfo(r.VirtualPath,
                    url: r.Url,
                    hasParameters: r.ParameterNames.Any(),
                    defaultValues: r.DefaultValues.Select(s => s.Key + ":" + s.Value).ToImmutableArray(),
                    routeName: r.RouteName)).ToImmutableArray();
        }

        public ImmutableArray<DotHtmlFileInfo> GetRoutes()
        {
            return routes.Value;
        }
        private IEnumerable<DotHtmlFileInfo> GetFilesToBeCompiled()
        {
            return Enumerable.Union(controls.Value, routes.Value).Where(t => t.Status == CompilationState.None);
        }

        static readonly SemaphoreSlim compilationSemaphore = new SemaphoreSlim(1);

        public async Task<bool> CompileAll(bool buildInParallel = true, bool forceRecompile = false)
        {
            var exclusiveMode = false;
            try
            {
                IEnumerable<DotHtmlFileInfo>? filesToCompile = null;
                if (forceRecompile)
                {
                    filesToCompile = controls.Value.Union(routes.Value);
                }
                else
                {
                    filesToCompile = GetFilesToBeCompiled();
                    if (filesToCompile.Any())
                    {
                        await compilationSemaphore.WaitAsync(); //LOCK

                        exclusiveMode = true;
                        filesToCompile = GetFilesToBeCompiled();
                        if (!filesToCompile.Any())
                            return GetFilesWithFailedCompilation().Any();
                    }
                }
                var discoveredMasterPages = new ConcurrentDictionary<string, DotHtmlFileInfo>();


                Func<DotHtmlFileInfo, Action> compilationTaskFactory = t => new Action(() => {
                    BuildView(t, out var masterPage);
                    if (masterPage != null && masterPage.Status == CompilationState.None) discoveredMasterPages.TryAdd(masterPage.VirtualPath, masterPage);
                });

                var compileTasks = filesToCompile.Select(compilationTaskFactory).ToArray();
                await ExecuteCompileTasks(compileTasks, buildInParallel);

                while (discoveredMasterPages.Any())
                {
                    compileTasks = discoveredMasterPages.Values.Select(compilationTaskFactory).ToArray();
                    discoveredMasterPages = new ConcurrentDictionary<string, DotHtmlFileInfo>();

                    await ExecuteCompileTasks(compileTasks, buildInParallel);
                }
            }
            finally
            {
                if (exclusiveMode) compilationSemaphore.Release(); //UNLOCK
            }

            return !GetFilesWithFailedCompilation().Any();
        }

        private async Task ExecuteCompileTasks(Action[] compileTasks, bool buildInParallel)
        {
            if (buildInParallel)
            {
                await Task.WhenAll(compileTasks.Select(Task.Run));
            }
            else
            {
                foreach (var task in compileTasks)
                {
                    task();
                }
            }
        }

        public bool BuildView(DotHtmlFileInfo file, out DotHtmlFileInfo? masterPage)
        {
            masterPage = null;
            if (file.Status != CompilationState.NonCompilable)
            {
                try
                {
                    var pageBuilder = controlBuilderFactory.GetControlBuilder(file.VirtualPath);

                    using var scopedServiceProvider = dotvvmConfiguration.ServiceProvider.CreateScope(); // dependencies that are configured as scoped cannot be resolved from root service provider
                    var compiledControl = pageBuilder.builder.Value.BuildControl(controlBuilderFactory, scopedServiceProvider.ServiceProvider);

                    if (pageBuilder.descriptor.MasterPage is { FileName: {} masterPagePath })
                    {
                        masterPage = masterPages.Value.GetOrAdd(masterPagePath, path => new DotHtmlFileInfo(path));
                    }

                    file.Status = CompilationState.CompletedSuccessfully;
                    file.Exception = null;
                }
                catch (Exception e)
                {
                    file.Status = CompilationState.CompilationFailed;
                    file.Exception = e.Message;
                    return false;
                }
            }
            return true;
        }

        /// <summary> Callback from the compiler which adds the view compilation result to the status page. </summary>
        public void RegisterCompiledView(string file, ViewCompiler.ControlBuilderDescriptor? descriptor, Exception? exception)
        {
            var fileInfo =
                routes.Value.FirstOrDefault(t => t.VirtualPath == file) ??
                controls.Value.FirstOrDefault(t => t.VirtualPath == file) ??
                masterPages.Value.GetOrAdd(file, path => new DotHtmlFileInfo(path));
            
            if (exception is null)
            {
                fileInfo.Status = CompilationState.CompletedSuccessfully;
                fileInfo.Exception = null;
            }
            else
            {
                fileInfo.Status = CompilationState.CompilationFailed;
                fileInfo.Exception = exception.Message;
            }

            if (descriptor?.MasterPage is { FileName: {} masterPagePath })
            {
                masterPages.Value.GetOrAdd(masterPagePath, path => new DotHtmlFileInfo(path));
            }
        }
    }
}
