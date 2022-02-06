using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Redirect;

namespace DotVVM.Samples.Common.Views.FeatureSamples.MarkupControl.ServiceDependency
{
    public class ServiceDependencyControl : DotvvmMarkupControl
    {
        public ServiceDependencyControl(ScopedTestService scopedService, SingletonTestService singletonService)
        {
        }
    }
}

