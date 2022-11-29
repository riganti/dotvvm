using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.Common.Api.Owin;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
    public class ApiInSpa_PageAViewModel : ApiInSpa_MasterViewModel
    {
        private readonly ResetClient owinResetApi;

        public ApiInSpa_PageAViewModel(ResetClient owinResetApi)
        {
            this.owinResetApi = owinResetApi;
        }
        
        public async Task Reset()
        {
            await owinResetApi.ResetDataAsync();
            Context.RedirectToRoute(Context.Route.RouteName, allowSpaRedirect: false);
        }
    }
}

