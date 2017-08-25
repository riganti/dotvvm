using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
	public class GetCollectionViewModel : DotvvmViewModelBase
	{
        public int SelectedCompanyId { get; set; } = -1;
        public int EditedOrderId { get; set; } = -1;
    }
}

