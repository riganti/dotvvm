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
        public int DependencyInstanceID2 { get; }

        public ViewModelScopedServiceViewModel(ViewModelScopedDependency dependency, IServiceProvider serviceProvider, DotvvmConfiguration configuration)
        {
            DependencyInstanceID = dependency.InstanceID;
            var v = serviceProvider.GetRequiredService<ViewModelScopedDependency>();
            DependencyInstanceID2 = v.InstanceID;
            // Check that the IServiceProvider has the same service as the service injected into constructor
            Debug.Assert(dependency.InstanceID == v.InstanceID);
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

