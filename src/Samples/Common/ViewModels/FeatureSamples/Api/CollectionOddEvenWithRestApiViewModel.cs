using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.Api.Common.Model;
using DotVVM.Samples.Common.Api.Owin;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
    public class CollectionOddEvenWithRestApiViewModel : DotvvmViewModelBase
    {
        private readonly ResetClient owinResetApi;

        public ICollection<Company<string>> Companies { get; set; }
        public int Value { get; set; }

        public CollectionOddEvenWithRestApiViewModel(ResetClient owinResetApi)
        {
            this.owinResetApi = owinResetApi;
        }

        public override Task Init()
        {
            owinResetApi.ResetData();
            return base.Init();
        }
    }
}

