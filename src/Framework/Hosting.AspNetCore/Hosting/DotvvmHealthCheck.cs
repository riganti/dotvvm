using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHealthCheck : IHealthCheck
    {
        private readonly IDotvvmViewCompilationService compilationService;
        private readonly DotvvmConfiguration configuration;

        public DotvvmHealthCheck(
            IDotvvmViewCompilationService compilationService,
            DotvvmConfiguration configuration
        )
        {
            this.compilationService = compilationService;
            this.configuration = configuration;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var c = compilationService.GetRoutes().AddRange(compilationService.GetControls().AddRange(compilationService.GetMasterPages()));

            if (!c.Any(p => p.Status == CompilationState.CompilationFailed))
            {
                return Task.FromResult(new HealthCheckResult(HealthStatus.Healthy));
            }

            // if nothing compiles, we say unhealthy. If at least something works, we say only degraded
            var state =
                c.Any(p => p.Status is CompilationState.CompletedSuccessfully or CompilationState.CompilationWarning)
                    ? HealthStatus.Degraded
                    : HealthStatus.Unhealthy;
            
            var views = c.Where(p => p.Status == CompilationState.CompilationFailed)
                         .Select(c => c.VirtualPath);
            var statusPageUrl = configuration.Diagnostics.CompilationPage.Url;
            return Task.FromResult(new HealthCheckResult(state, $"Dothtml pages can not be compiled: {views.Take(10).StringJoin(", ")}. See {statusPageUrl} for more information."));
        }

        public static void RegisterHealthCheck(IServiceCollection services)
        {
            services.ConfigureWithServices<HealthCheckServiceOptions>((options, s) => {
                if (options.Registrations.Any(c => c.Name == "DotVVM"))
                    return;

                options.Registrations.Add(
                    new HealthCheckRegistration(
                        "DotVVM",
                        ActivatorUtilities.CreateInstance<DotvvmHealthCheck>(s),
                        null,
                        new [] { "dotvvm" }
                    )
                );
            });
        }
    }
}
