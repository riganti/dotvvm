using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Diagnostics.StatusPage
{
    public class StatusViewModel : DotvvmViewModelBase
    {
        public List<DotHtmlFileInfo> Routes { get; set; }

        public List<DotHtmlFileInfo> Controls { get; set; }
        public string ApplicationPath { get; set; }

        public override Task Init()
        {
            // ToDo: implement your own authorization logic
            if (!Context.HttpContext.Request.Url.IsLoopback)
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
            return base.Init();
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
                    DefaultValues = r.DefaultValues,
                    RouteName = r.RouteName,
                    Status = string.IsNullOrWhiteSpace(r.VirtualPath) ? CompilationState.NonCompilable : CompilationState.None
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

        public void CompileAll()
        {
            Routes.ForEach(BuildView);
            Controls.ForEach(BuildView);
            MasterPages.ForEach(BuildView);
        }

        public void BuildView(DotHtmlFileInfo file)
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
                        if (MasterPages.All(s => s.VirtualPath != masterPage))
                        {
                            MasterPages.Add(new DotHtmlFileInfo()
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
        public IDictionary<string, object> DefaultValues { get; set; }

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
        NonCompilable = 6
    }
}