using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Validation
{
    public class ClientSideObservableUpdateViewModel : DotvvmViewModelBase
    {

        public Test Test1 { get; set; }
        public Test Test2 { get; set; }

        public void SwitchTests()
        {
            if (Test1 == null)
            {
                Test2 = null;
                Test1 = new Test { Text = "lol" };
            }
            else
            {
                Test1 = null;
                Test2 = new Test { Text = "" };
            }
        }


        public class Test
        {
            [Required, EmailAddress]
            public string Text { get; set; }
        }

    }
}

