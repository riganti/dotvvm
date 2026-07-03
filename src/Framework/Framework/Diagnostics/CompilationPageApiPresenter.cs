using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Diagnostics
{
    internal class CompilationPageApiPresenter : IDotvvmPresenter
    {
        private readonly IDotvvmViewCompilationService compilationService;

        public CompilationPageApiPresenter(IDotvvmViewCompilationService compilationService)
        {
            this.compilationService = compilationService;
        }

        public async Task ProcessRequest(IDotvvmRequestContext context)
        {
            var response = context.HttpContext.Response;
            var isAuthorized = await context.Configuration.Diagnostics.CompilationPage.AuthorizationPredicate(context);
            if (!isAuthorized)
            {
                response.StatusCode = 403;
                return;
            }

            var result = await DotvvmCompilationStatusApi.GetStatusResponse(compilationService);
            response.StatusCode = result.StatusCode;
            if (result.ResponseBody is null)
            {
                return;
            }

            response.ContentType = "application/json; charset=utf-8";
            await response.WriteAsync(result.ResponseBody);
        }
    }
}
