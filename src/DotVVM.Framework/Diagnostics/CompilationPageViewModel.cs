using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Diagnostics
{
    public class CompilationPageViewModel : DotvvmViewModelBase
    {
        private readonly IDotvvmViewCompilationService viewCompilationService;

        public ImmutableArray<DotHtmlFileInfo> Routes => viewCompilationService.GetRoutes();
        public ImmutableArray<DotHtmlFileInfo> MasterPages => viewCompilationService.GetMasterPages();
        public ImmutableArray<DotHtmlFileInfo> Controls => viewCompilationService.GetControls();
        public string? ApplicationPath { get; set; }
        public bool CompileAfterLoad { get; set; }
        public int ActiveTab { get; set; } = 0;

        public CompilationPageViewModel(IDotvvmViewCompilationService viewCompilationService)
        {
            this.viewCompilationService = viewCompilationService;
        }

        public override async Task Init()
        {
            ApplicationPath = Context.Configuration.ApplicationPhysicalPath;
            CompileAfterLoad = Context.Configuration.Development.CompilationPage.ShouldCompileAllOnLoad;

            await base.Init();
        }

        public async Task CompileAll()
        {
            await viewCompilationService.CompileAll(true, true);
        }

        public void BuildView(DotHtmlFileInfo file)
        {
            viewCompilationService.BuildView(file, out _);
        }
    }
}
