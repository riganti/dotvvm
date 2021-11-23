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
        public int ActiveTab { get; set; } = 0;

        public CompilationPageViewModel(IDotvvmViewCompilationService viewCompilationService)
        {
            this.viewCompilationService = viewCompilationService;
        }

        public override async Task Init()
        {
            var isAuthorized = await Context.Configuration.Diagnostics.CompilationPage.AuthorizationPredicate(Context);
            if (!isAuthorized)
            {
                await Context.RejectRequest("Unauthorized access to compilation page");
                return;
            }

            await base.Init();
        }

        public async Task CompileAll(bool forceRecompile)
        {
            await viewCompilationService.CompileAll(buildInParallel: true, forceRecompile: forceRecompile);
        }

        public void BuildView(DotHtmlFileInfo file)
        {
            viewCompilationService.BuildView(file, out _);
        }
    }
}
