using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Resources;
using DotVVM.Framework.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Diagnostics.StatusPage
{
    public class DotvvmViewCompilationService : IDotvvmViewCompilationService
    {
        //this will be removed during integration of this service into DotVVM
        public bool BuildInParallel { get; set; }
        private readonly DotvvmConfiguration dotvvmConfiguration;

        private bool EverythingIsCompiled => controls!=null && routes!=null && masterPages !=null && masterPages.Concat(controls).Concat(routes).All(t=>t.Status!=CompilationState.None);
        public  IEnumerable<DotHtmlFileInfo> FilesWithErrors=> !EverythingIsCompiled ? Enumerable.Empty<DotHtmlFileInfo>():
            masterPages.Concat(controls).Concat(routes).Where(t=>t.Status==CompilationState.CompilationFailed);

        private List<DotHtmlFileInfo> masterPages;
        public List<DotHtmlFileInfo> GetMasterPages(){

            if (masterPages == null) InitMasterPagesCollection();
            return masterPages;
        }

        private List<DotHtmlFileInfo> controls;
        public List<DotHtmlFileInfo> GetControls()
        {
            if (controls == null)
            {
                lock (loadControlsLocker)
                {
                    if (controls == null)
                        controls = dotvvmConfiguration.Markup.Controls.Where(s => !string.IsNullOrWhiteSpace(s.Src))
                            .Select(s => new DotHtmlFileInfo()
                            {
                                TagName = s.TagName,
                                VirtualPath = s.Src,
                                Namespace = s.Namespace,
                                Assembly = s.Assembly,
                                TagPrefix = s.TagPrefix,
                                Status = CompilationState.None
                            }).ToList();
                }
            }
            return controls;
        }

        protected List<DotHtmlFileInfo> routes;
        public List<DotHtmlFileInfo> GetRoutes()
        {
            if (routes == null)
            {

                if (routes == null)
                {
                    lock (loadRoutesLocker)
                    {
                        if (routes == null)
                            routes = dotvvmConfiguration.RouteTable.Select(r => new DotHtmlFileInfo()
                            {
                                VirtualPath = r.VirtualPath,
                                Url = r.Url,
                                HasParameters = r.ParameterNames.Any(),
                                DefaultValues = r.DefaultValues.Select(s => s.Key + ":" + s.Value?.ToString()).ToList(),
                                RouteName = r.RouteName,
                                Status = (string.IsNullOrWhiteSpace(r.VirtualPath) || !IsDotvvmPresenter(r))
                                    ? CompilationState.NonCompilable
                                    : CompilationState.None
                            }).ToList();
                    }
                }
            }
            return routes;
        }

        public DotvvmViewCompilationService(DotvvmConfiguration dotvvmConfiguration)
        {
            this.dotvvmConfiguration = dotvvmConfiguration;
        }
        
        private static object loadMasterPagesLocker = new object();
        private void InitMasterPagesCollection()
        {
            if (masterPages == null)
            {
                lock (loadMasterPagesLocker)
                {
                    if (masterPages == null) masterPages = new List<DotHtmlFileInfo>();
                }
            }
        }

        private static object loadControlsLocker = new object();
        private static object loadRoutesLocker = new object();

        private bool IsDotvvmPresenter(RouteBase r)
        {
            var presenter = r.GetPresenter(dotvvmConfiguration.ServiceProvider);
            return presenter.GetType().IsInstanceOfType(dotvvmConfiguration.RouteTable.GetDefaultPresenter(dotvvmConfiguration.ServiceProvider));
        }

        public async Task<bool> CompileAll(bool forceRecompile=false)
        {
            if (EverythingIsCompiled && !forceRecompile)
                return !FilesWithErrors.Any();
            
            List<DotHtmlFileInfo> controlsToCompile;
            List<DotHtmlFileInfo> routesToCompile;
            if (forceRecompile)
            {
                routesToCompile = routes;
                controlsToCompile = controls;
            }
            else
            {
                routesToCompile = routes.Where(t => t.Status == CompilationState.None).ToList();
                controlsToCompile = controls.Where(t => t.Status == CompilationState.None).ToList();
            }


            var tempMasterPages = new ConcurrentBag<DotHtmlFileInfo>();
            var compileTasks = routesToCompile.Select(a => new Task(() => BuildView(a, tempMasterPages))).ToList();

            compileTasks.AddRange(controlsToCompile.Select(a => new Task(() => BuildView(a, tempMasterPages))).ToList());

            await ExecuteCompileTasks(compileTasks);

            while (tempMasterPages.Count > 0)
            {
                masterPages = masterPages.Union(tempMasterPages).Distinct(DotHtmlFileInfo.VirtualPathComparer).ToList();

                //.NET Standard 2.1 - better replace with tempMasterPages.Clear()
                tempMasterPages = new ConcurrentBag<DotHtmlFileInfo>();

                var masterPagesTasks = masterPages.Select(i => new Task(() => BuildView(i, tempMasterPages))).ToList();
                await ExecuteCompileTasks(masterPagesTasks);
            }

            OrderByErrors();
            return !FilesWithErrors.Any();
        }

        private async Task ExecuteCompileTasks(List<Task> compileTasks)
        {
            if (BuildInParallel)
            {
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

        protected virtual void OrderByErrors()
        {
            routes = routes.OrderByDescending(r => r.Status).ToList();
            masterPages = masterPages.OrderByDescending(mp => mp.Status).ToList();
            controls = controls.OrderByDescending(c => c.Status).ToList();
        }

        public bool BuildView(DotHtmlFileInfo file, ConcurrentBag<DotHtmlFileInfo> tempList)
        {

            Debug.WriteLine($"Precompiling path: {file?.VirtualPath}");
            if (file.Status != CompilationState.NonCompilable)
            {
                try
                {
                    var controlFactory = dotvvmConfiguration.ServiceProvider.GetRequiredService<IControlBuilderFactory>();

                    var pageBuilder = controlFactory.GetControlBuilder(file.VirtualPath);

                    var compiledControl = pageBuilder.builder.Value.BuildControl(controlFactory, dotvvmConfiguration.ServiceProvider);

                    if (compiledControl is DotvvmView view && view.Directives.TryGetValue(
                        ParserConstants.MasterPageDirective,
                        out var masterPage))
                    {
                        if (masterPages.All(s => s.VirtualPath != masterPage) &&
                            tempList.All(s => s.VirtualPath != masterPage))
                        {
                            tempList.Add(new DotHtmlFileInfo()
                            {
                                VirtualPath = masterPage
                            });
                        }
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