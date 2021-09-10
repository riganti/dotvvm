﻿using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

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

            var result = await compilationService.CompileAll(buildInParallel: true);
            if (result)
            {
                response.StatusCode = 200;
                return;
            }

            response.StatusCode = 500;
            response.ContentType = "application/json";
            await response.WriteAsync(JsonConvert.SerializeObject(compilationService.GetFilesWithFailedCompilation()));
        }
    }
}
