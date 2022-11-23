﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
    public class ApiRefreshViewModel : DotvvmViewModelBase
    {
        public int CompanyId { get; set; } = 1;
        public int NumberOfRequests { get; set; }

    }
}

