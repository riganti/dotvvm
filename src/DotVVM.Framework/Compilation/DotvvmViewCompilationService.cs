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
            controls = new Lazy<ConcurrentBag<DotHtmlFileInfo>>(InitControls);
            routes = new Lazy<ConcurrentBag<DotHtmlFileInfo>>(InitRoutes);
        }

        public ImmutableArray<DotHtmlFileInfo> GetFilesWithFailedCompilation()
        {
            return masterPages.Value.Values.Concat(controls.Value).Concat(routes.Value)
                .Where(t => t.Status == CompilationState.CompilationFailed).ToImmutableArray();
        }

        private Lazy<ConcurrentDictionary<string, DotHtmlFileInfo>> masterPages;
        private ConcurrentDictionary<string, DotHtmlFileInfo> InitMasterPagesCollection()
        {
            return new ConcurrentDictionary<string, DotHtmlFileInfo>();
        }
        public ImmutableArray<DotHtmlFileInfo> GetMasterPages()
        {
            return masterPages.Value.Values.ToImmutableArray();
        }

        private Lazy<ConcurrentBag<DotHtmlFileInfo>> controls;
        private ConcurrentBag<DotHtmlFileInfo> InitControls()
        {
            return new ConcurrentBag<DotHtmlFileInfo>(
                dotvvmConfiguration.Markup.Controls.Where(s => !string.IsNullOrWhiteSpace(s.Src))
                    .Select(s => new DotHtmlFileInfo(s.Src, tagPrefix: s.TagPrefix, tagName: s.TagName,
                        nameSpace: s.Namespace, assembly: s.Assembly)));
        }

        public ImmutableArray<DotHtmlFileInfo> GetControls()
        {
            return controls.Value.ToImmutableArray();
        }

        private Lazy<ConcurrentBag<DotHtmlFileInfo>> routes;
        private ConcurrentBag<DotHtmlFileInfo> InitRoutes()
        {
            return new ConcurrentBag<DotHtmlFileInfo>(dotvvmConfiguration.RouteTable.Select(r =>
                new DotHtmlFileInfo(r.VirtualPath,
                    url: r.Url,
                    hasParameters: r.ParameterNames.Any(),
                    defaultValues: r.DefaultValues.Select(s => s.Key + ":" + s.Value).ToImmutableArray(),
                    routeName: r.RouteName)));
        }

        public ImmutableArray<DotHtmlFileInfo> GetRoutes()
        {
            return routes.Value.ToImmutableArray();
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
                IEnumerable<DotHtmlFileInfo> filesToCompile = null;
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


                Func<DotHtmlFileInfo, Task> CreateCompilationTask()
                {
                    return t => new Task(() => {
                        BuildView(t, out var masterPage);
                        if (masterPage != null && masterPage.Status == CompilationState.None) discoveredMasterPages.TryAdd(masterPage.VirtualPath, masterPage);
                    });
                }

                var compileTasks = filesToCompile.Select(CreateCompilationTask()).ToArray();
                await ExecuteCompileTasks(compileTasks, buildInParallel);

                while (discoveredMasterPages.Any())
                {
                    compileTasks = discoveredMasterPages.ToArray().Select(t => t.Value).Select(CreateCompilationTask()).ToArray();
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

        private async Task ExecuteCompileTasks(ICollection<Task> compileTasks, bool buildInParallel)
        {
            if (buildInParallel)
            {
                foreach (var task in compileTasks)
                {
                    task.Start();
                }
                await Task.WhenAll(compileTasks.ToArray());
            }
            else
            {
                foreach (var task in compileTasks)
                {
                    task.RunSynchronously();
                }
            }
        }

        public bool BuildView(DotHtmlFileInfo file, out DotHtmlFileInfo masterPage)
        {
            masterPage = null;
            if (file.Status != CompilationState.NonCompilable)
            {
                try
                {
                    var pageBuilder = controlBuilderFactory.GetControlBuilder(file.VirtualPath);

                    var compiledControl = pageBuilder.builder.Value.BuildControl(controlBuilderFactory, dotvvmConfiguration.ServiceProvider);

                    if (compiledControl is DotvvmView view &&
                        view.Directives.TryGetValue(ParserConstants.MasterPageDirective, out var masterPagePath))
                    {
                        masterPage = masterPages.Value.GetOrAdd(masterPagePath, new DotHtmlFileInfo(masterPagePath));
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
    }
}
