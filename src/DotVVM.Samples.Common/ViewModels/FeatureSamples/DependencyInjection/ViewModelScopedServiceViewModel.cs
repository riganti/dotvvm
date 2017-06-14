using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Configuration;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.DependencyInjection
{
    public class ViewModelScopedServiceViewModel : DotvvmViewModelBase
    {
        public int DependencyInstanceID { get; }

        public ViewModelScopedServiceViewModel(ViewModelScopedDependency dependency, IServiceProvider serviceProvider)
        {
            DependencyInstanceID = dependency.InstanceID;
            var v = serviceProvider.GetService<ViewModelScopedDependency>();
            Debug.Assert(dependency.InstanceID != v.InstanceID);
        }
    }

    public class ViewModelScopedDependency
    {
        static int _instanceCounter;

        public int InstanceID { get; }

        public DotvvmConfiguration Configuration { get; }

        public ViewModelScopedDependency(DotvvmConfiguration config)
        {
            InstanceID = System.Threading.Interlocked.Add(ref _instanceCounter, 1);
            Configuration = config;
        }
    }
}

