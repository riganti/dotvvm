using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Diagnostics.StatusPage
{
    public class StatusPageApiPresenter : IDotvvmPresenter
    {
        private readonly StatusPageApiOptions options;
        private readonly IDotvvmViewCompilationService compilationService;

        public StatusPageApiPresenter(StatusPageApiOptions options, IDotvvmViewCompilationService compilationService)
        {
            this.options = options;
            this.compilationService = compilationService;
        }
        public async Task ProcessRequest(IDotvvmRequestContext context)
        {
            var authorized = await options.Authorize(context);
            var httpContext = context.HttpContext;
            var response = httpContext.Response;
            
            if (!authorized && options.NonAuthorizedApiAccessMode==NonAuthorizedApiAccessMode.Deny)
            {
                response.StatusCode = 401;
                return;
            }

            var result = await compilationService.CompileAll(false);
            if (result)
            {
                response.StatusCode = 200;
                return;
            }
            else
            {
                response.StatusCode = 500;
            }

            if (authorized || options.NonAuthorizedApiAccessMode == NonAuthorizedApiAccessMode.DetailedResponse)
            {
                response.ContentType = "application/json";
                await response.WriteAsync(JsonConvert.SerializeObject(compilationService.GetFilesWithFailedCompilation()));
            }
        }
    }
}