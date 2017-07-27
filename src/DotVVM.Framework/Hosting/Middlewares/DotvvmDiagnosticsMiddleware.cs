using System;
using System.IO;
using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics;
using DotVVM.Framework.Diagnostics.Models;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmDiagnosticsMiddleware : IMiddleware
    {
        private readonly DotvvmDiagnosticsConfiguration configuration;
        private readonly DiagnosticsDataSender sender;

        public DotvvmDiagnosticsMiddleware()
        {
            configuration = new DotvvmDiagnosticsConfiguration();
            sender = new DiagnosticsDataSender(configuration);
        }

        public async Task<bool> Handle(IDotvvmRequestContext request)
        {
            
            try
            {
                if(request.IsPostBack || request.ViewModel != null)
                    await sender.SendDataAsync(RequestContextToDiagnosticsData(request));
            }
            catch
            {
                return false;
            }
            return true;
        }

        private DiagnosticsData RequestContextToDiagnosticsData(IDotvvmRequestContext request)
        {
            return new DiagnosticsData
            {
                RequestDiagnostics = HttpRequestDiagnostics.FromDotvvmRequestContext(request),
                ResponseDiagnostics = HttpResponseDiagnostics.FromDotvvmRequestContext(request)
            };
        }
    }
}