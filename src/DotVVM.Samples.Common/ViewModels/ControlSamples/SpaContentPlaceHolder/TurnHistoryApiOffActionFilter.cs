using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.SpaContentPlaceHolder
{
    public class TurnHistoryApiOffActionFilter : ActionFilterAttribute
    {
        protected override Task OnPageLoadingAsync(IDotvvmRequestContext context)
        {
            context.Configuration.UseHistoryApiSpaNavigation = false;

            return base.OnPageLoadingAsync(context);
        }

        protected override Task OnPageLoadedAsync(IDotvvmRequestContext context)
        {
            context.Configuration.UseHistoryApiSpaNavigation = true;

            return base.OnPageLoadedAsync(context);
        }
    }
}
