using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Validation
{
    public class ModelStateErrorsViewModel : DotvvmViewModelBase
    {
        public NestedVM1 Nested1 { get; set; } = new NestedVM1 {
            MyProperty = "FFF"
        };
        public List<NestedVM2> Nested2 { get; set; } = new List<NestedVM2> {
            new NestedVM2 {
                Property123 = "a"
            },
            new NestedVM2 {
                Property123 = "b"
            },
            new NestedVM2 {
                Property123 = "c"
            }
        };

        public string Property456 { get; set; }

        public void Command1()
        {
            this.AddModelError(v => v.Property456, "Property456 contains error");
            Context.FailOnInvalidModelState();
        }

        public void Command2()
        {
            this.AddModelError(v => v.Nested1.MyProperty, "MyProperty contains error");
            Context.FailOnInvalidModelState();
        }

        public void Command3()
        {
            this.AddModelError(v => v.Nested2[2].Property123, "Property123 contains error");
            this.AddModelError(v => v.Nested2[0].Property123, "Property123 contains error");
            Context.FailOnInvalidModelState();
        }

        public class NestedVM1 : DotvvmViewModelBase
        {
            public string MyProperty { get; set; }
        }

        public class NestedVM2 : DotvvmViewModelBase
        {
            public string Property123 { get; set; }
        }
    }
}

