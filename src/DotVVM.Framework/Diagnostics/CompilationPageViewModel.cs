using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Diagnostics
{
    public class CompilationPageViewModel
        : DotvvmViewModelBase
    {
        private readonly DotvvmConfiguration config;
        private readonly IDotvvmViewCompilationService viewCompilationService;

        public ImmutableArray<DotHtmlFileInfo> Routes => viewCompilationService.GetRoutes();
        public ImmutableArray<DotHtmlFileInfo> MasterPages => viewCompilationService.GetMasterPages();
        public ImmutableArray<DotHtmlFileInfo> Controls => viewCompilationService.GetControls();
        public string ApplicationPath { get; set; }
        public bool CompileAfterLoad { get; set; }

        public CompilationPageViewModel(DotvvmConfiguration config, IDotvvmViewCompilationService viewCompilationService)
        {
            this.config = config;
            this.viewCompilationService = viewCompilationService;
        }

        public override async Task Init()
        {
            ApplicationPath = Context.Configuration.ApplicationPhysicalPath;
            CompileAfterLoad = config.Development.CompilationPage.ShouldCompileAllOnLoad;

            await base.Init();
        }

        public async Task CompileAll()
        {
            await viewCompilationService.CompileAll(true,true);
        }

        public void BuildView(DotHtmlFileInfo file)
        {
            viewCompilationService.BuildView(file, out _);
        }
    }
}
