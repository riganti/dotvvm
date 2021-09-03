using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Formatting
{
    public class FormattingViewModel : DotvvmViewModelBase
    {

        public decimal Number { get; set; } = 123456789.123m;

        public DateTime Date { get; set; } = new DateTime(1234, 5, 6, 7, 8, 9, DateTimeKind.Local);

        public DateTime? Null => null;

        public override Task Init()
        {
            Context.ChangeCurrentCulture("en-US");
            return base.Init();
        }

        public void PostBack()
        {
            Number = 23456789.123m;
            Date = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local);
        }
    }

}