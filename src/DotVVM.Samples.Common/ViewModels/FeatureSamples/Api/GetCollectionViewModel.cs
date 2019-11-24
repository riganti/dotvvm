using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.Common.Api.Owin;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
	public class GetCollectionViewModel : DotvvmViewModelBase
	{
        private readonly ResetClient owinResetApi;


        public int SelectedCompanyId { get; set; } = -1;
        public int EditedOrderId { get; set; } = -1;


        public GetCollectionViewModel(ResetClient owinResetApi)
        {
            this.owinResetApi = owinResetApi;
        }

        public override async Task Load()
        {
            await owinResetApi.ResetDataAsync();
            await base.Load();
        }
    }
}

