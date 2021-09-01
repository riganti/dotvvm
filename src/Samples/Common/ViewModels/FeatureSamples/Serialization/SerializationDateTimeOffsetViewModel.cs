using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Serialization
{
    public class SerializationDateTimeOffsetViewModel : DotvvmViewModelBase
    {

        public DateTimeOffset Offset { get; set; } = new DateTimeOffset(2000, 1, 2, 3, 4, 5, 0, TimeSpan.Zero);

        public DateTimeOffset? NullableOffset { get; set; }


        public void AddHour()
        {
            Offset = Offset.AddHours(1);
            NullableOffset = NullableOffset?.AddHours(1);
        }

        public void SetNullable()
        {
            NullableOffset = new DateTimeOffset(2000, 1, 2, 3, 4, 5, 0, TimeSpan.FromHours(2));
        }

    }
}

