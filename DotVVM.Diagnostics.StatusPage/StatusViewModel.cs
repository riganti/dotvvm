using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Diagnostics.StatusPage
{
    public class StatusViewModel : DotvvmViewModelBase
    {
        private readonly StatusPageOptions _statusPageOptions;
        private readonly Type DotvvmPresenterType = typeof(DotvvmPresenter);
        public List<DotHtmlFileInfo> Routes { get; set; }

        public List<DotHtmlFileInfo> Controls { get; set; }
        public string ApplicationPath { get; set; }
        public bool CompileAfterLoad { get; set; }

        public StatusViewModel(StatusPageOptions statusPageOptions)
        {
            _statusPageOptions = statusPageOptions;
        }

        public override async Task Init()
        {
            var isAuthorized = await _statusPageOptions.Authorize(Context);
            if (!isAuthorized)
            {
                var response = Context.HttpContext.Response;
                response.StatusCode = 403;

                Context.InterruptRequest();
            }

            if (!Context.IsPostBack)
            {
                MasterPages = new List<DotHtmlFileInfo>();
            }

            ApplicationPath = Context.Configuration.ApplicationPhysicalPath;
            CompileAfterLoad = _statusPageOptions.CompileAfterPageLoads;
            await base.Init();
        }

        public override Task Load()
        {
            if (!Context.IsPostBack)
            {
                Routes = Context.Configuration.RouteTable.Select(r => new DotHtmlFileInfo()
                {
                    VirtualPath = r.VirtualPath,
                    Url = r.Url,
                    HasParameters = r.ParameterNames.Any(),
                    DefaultValues = r.DefaultValues.Select(s=> s.Key + ":" + s.Value?.ToString()).ToList(),
                    RouteName = r.RouteName,
                    Status = (string.IsNullOrWhiteSpace(r.VirtualPath) || !IsDotvvmPresenter(r))
                        ? CompilationState.NonCompilable
                        : CompilationState.None
                }).ToList();

                Controls = Context.Configuration.Markup.Controls.Where(s => !string.IsNullOrWhiteSpace(s.Src))
                    .Select(s => new DotHtmlFileInfo()
                    {
                        TagName = s.TagName,
                        VirtualPath = s.Src,
                        Namespace = s.Namespace,
                        Assembly = s.Assembly,
                        TagPrefix = s.TagPrefix
                    }).ToList();
            }

            return base.Load();
        }

        private bool IsDotvvmPresenter(RouteBase r)
        {
            var presenter = r.GetPresenter(Context.Services);
            return presenter.GetType().IsAssignableFrom(DotvvmPresenterType);
        }

        public override Task PreRender()
        {
            if (Context.IsPostBack)
            {
                Routes = Routes.OrderByDescending(r => r.Status).ToList();
                MasterPages = MasterPages.OrderByDescending(mp => mp.Status).ToList();
                Controls = Controls.OrderByDescending(c => c.Status).ToList();
            }

            return base.PreRender();
        }

        public List<DotHtmlFileInfo> MasterPages { get; set; }

        public async Task CompileAll()
        {
            var tempMasterPages = new ConcurrentBag<DotHtmlFileInfo>();
            var compileTasks = Routes.Select(a => Task.Run(() => BuildView(a, tempMasterPages))).ToList();

            compileTasks.AddRange(Controls.Select(a => Task.Run(() => BuildView(a, tempMasterPages))).ToList());

            await Task.WhenAll(compileTasks.ToArray());

            while (tempMasterPages.Count > 0)
            {
                tempMasterPages = new ConcurrentBag<DotHtmlFileInfo>(tempMasterPages.Distinct());
                MasterPages.AddRange(tempMasterPages);

                //.NET Standard 2.1 - better replace with tempMasterPages.Clear()
                tempMasterPages = new ConcurrentBag<DotHtmlFileInfo>();

                var masterPagesTasks = MasterPages.Select(i => Task.Run(() => BuildView(i, tempMasterPages))).ToList();

                await Task.WhenAll(masterPagesTasks.ToArray());
            }
        }

        public void BuildView(DotHtmlFileInfo file)
        {
            BuildView(file, new ConcurrentBag<DotHtmlFileInfo>(MasterPages));
        }

        private void BuildView(DotHtmlFileInfo file, ConcurrentBag<DotHtmlFileInfo> tempList)
        {
            if (file.Status != CompilationState.NonCompilable)
            {
                try
                {
                    var controlFactory = Context.Services.GetRequiredService<IControlBuilderFactory>();

                    var pageBuilder = controlFactory.GetControlBuilder(file.VirtualPath);

                    var compiledControl = pageBuilder.builder.Value.BuildControl(controlFactory, Context.Services);

                    if (compiledControl is DotvvmView view && view.Directives.TryGetValue(
                            ParserConstants.MasterPageDirective,
                            out var masterPage))
                    {
                        if (MasterPages.All(s => s.VirtualPath != masterPage) &&
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
                }
            }
        }
    }


    public class DotHtmlFileInfo
    {
        public CompilationState Status { get; set; }
        public string Exception { get; set; }
        public string TagName { get; set; }
        public string Namespace { get; set; }
        public string Assembly { get; set; }
        public string TagPrefix { get; set; }
        public string Url { get; set; }

        /// <summary>Gets key of route.</summary>
        public string RouteName { get; set; }

        /// <summary>Gets the default values of the optional parameters.</summary>
        public List<string> DefaultValues { get; set; }

        /// <summary>Gets or sets the virtual path to the view.</summary>
        public string VirtualPath { get; set; }

        public bool HasParameters { get; set; }
    }

    public enum CompilationState
    {
        None = 1,
        InProcess = 2,
        CompletedSuccessfully = 3,
        CompilationFailed = 4,
        CompilationWarning = 5,
        NonCompilable = 6
    }
}
