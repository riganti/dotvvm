using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ViewModelDeserialization
{
    public class PropertyNullAssignmentViewModel : DotvvmViewModelBase
    {

        public DateTime? NullableDateTime { get; set; }

        public DateTime Value { get; set; } = new DateTime(2023, 1, 2, 3, 4, 5, 678);

        public void SetNullableDateTimeToNull()
        {
            NullableDateTime = null;
        }
    }
}

