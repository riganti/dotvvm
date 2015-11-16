using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Formatting
{
    public class FormattingViewModel : DotvvmViewModelBase
    {

        public decimal Number => 123456789.123m;

        public DateTime Date => new DateTime(1234, 5, 6, 7, 8, 9, DateTimeKind.Local);

        public DateTime? Null => null;

        public override Task Init()
        {
            Context.ChangeCurrentCulture("en-US");
            return base.Init();
        }
    }

}