using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Diagnostics.StatusPage
{
    public class StatusViewModel : DotvvmViewModelBase
    {
        private readonly StatusPageOptions _statusPageOptions;
        private readonly IDotvvmViewCompilationService viewCompilationService;
        public ImmutableArray<DothtmlFileInfo> Routes => viewCompilationService.GetRoutes();
        public ImmutableArray<DothtmlFileInfo> MasterPages => viewCompilationService.GetMasterPages();
        public ImmutableArray<DothtmlFileInfo> Controls => viewCompilationService.GetControls();
        public string ApplicationPath { get; set; }
        public bool CompileAfterLoad { get; set; }

        public StatusViewModel(StatusPageOptions statusPageOptions, IDotvvmViewCompilationService viewCompilationService)
        {
            _statusPageOptions = statusPageOptions;
            this.viewCompilationService = viewCompilationService;
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

            ApplicationPath = Context.Configuration.ApplicationPhysicalPath;
            CompileAfterLoad = _statusPageOptions.CompileAfterPageLoads;

            await base.Init();
        }

        public async Task CompileAll()
        {
            await viewCompilationService.CompileAll(true,true);
        }

        public void BuildView(DothtmlFileInfo file)
        {
            viewCompilationService.BuildView(file, out _);
        }
    }
}
