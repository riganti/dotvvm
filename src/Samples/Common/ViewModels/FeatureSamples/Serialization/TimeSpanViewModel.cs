using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Serialization
{
    public class TimeSpanViewModel : DotvvmViewModelBase
    {

        public TimeSpan Time { get; set; }

        public TimeSpan? NullableTime { get; set; }

        public void AddHour()
        {
            Time = Time.Add(TimeSpan.FromHours(1));
            NullableTime = NullableTime?.Add(TimeSpan.FromHours(1));
        }


        public override Task PreRender()
        {
            Context.ResourceManager.AddCurrentCultureGlobalizationResource();

            return base.PreRender();
        } 
    }
}

