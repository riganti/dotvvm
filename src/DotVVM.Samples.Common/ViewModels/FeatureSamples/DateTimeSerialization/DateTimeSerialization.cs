using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.DateTimeSerialization
{
    public class DateTimeSerializationViewModel : DotvvmViewModelBase
    {
        public DateTime Date1 { get; set; }

        public string Date1String { get; set; }

        public void VerifyDate1()
        {
            Date1String = Date1.ToString("g");
        }

        public void SetDate1()
        {
            Date1 = DateTime.Now;
        }

        public DateTime? Date2 { get; set; }

        public string Date2String { get; set; } = "null";

        public void VerifyDate2()
        {
            Date2String = Date2?.ToString("g") ?? "null";
        }

        public void SetDate2()
        {
            Date2 = DateTime.Now;
        }

        public DateTime? Date3 { get; set; }

        public void SetStaticDate()
        {
            Date3 = new DateTime(2000, 1, 1);
        }
    }
}
