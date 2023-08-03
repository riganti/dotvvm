using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes
{
    public class RouteLinkViewModel : DotvvmViewModelBase
    {

        public SampleId Id1 { get; set; } = new SampleId(new Guid("D7682DE1-B985-4B4B-B2BF-C349192AD9C9"));

        public SampleId Id2 { get; set; } = new SampleId(new Guid("6F5E8011-BD12-477D-9E82-A7A1CE836773"));

        public SampleId Null { get; set; }

        public void ChangeIds()
        {
            Null = Id1;
            Id1 = Id2;
            Id2 = null;
        }
    }
}

