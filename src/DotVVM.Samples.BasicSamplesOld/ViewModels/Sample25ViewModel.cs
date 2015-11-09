using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample25ViewModel : DotvvmViewModelBase
    {

        [JsonConverter(typeof(DateTimeConverter1))]
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


        [JsonConverter(typeof(DateTimeConverter2))]
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

    }

    public class DateTimeConverter1 : DateTimeFormatJsonConverterBase
    {
        public override string[] DateTimeFormats => new[] { "d.M.yyyy" };
    }

    public class DateTimeConverter2 : DateTimeFormatJsonConverterBase
    {
        public override string[] DateTimeFormats => new[] { "yyyy-MM-dd HH:mm:ss" };
    }
    
}
