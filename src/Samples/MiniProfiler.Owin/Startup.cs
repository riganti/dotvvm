using System.Web.Hosting;
using Microsoft.Owin;
using Owin;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Tracing.MiniProfiler.Owin;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using DotVVM.Samples.MiniProfiler.Owin.ViewModels;

[assembly: OwinStartup(typeof(DotVVM.Samples.MiniProfiler.Owin.Startup))]

namespace DotVVM.Samples.MiniProfiler.Owin
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var applicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            //StackExchange.Profiling.WebRequestProfilerProvider.Setup("~/profiler",
            //// ResultsAuthorize (optional - open to all by default):
            //// because profiler results can contain sensitive data (e.g. sql queries with parameter values displayed), we
            //// can define a function that will authorize clients to see the json or full page results.
            //// we use it on https://stackoverflow.com to check that the request cookies belong to a valid developer.
            //request => request.IsLocal,
            //// ResultsListAuthorize (optional - open to all by default)
            //// the list of all sessions in the store is restricted by default, you must return true to allow it
            //request => request.IsLocal
            //);

            StackExchange.Profiling.MiniProfiler.Configure(new MiniProfilerOptions()
            {
                ResultsAuthorize = s => s.IsLocal,
                ResultsListAuthorize = s => true,
            });

            // use DotVVM
            var dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath);
        }
    }
}
