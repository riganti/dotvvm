using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.CheckBox;
using static DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.CheckBox.CheckedItemsRepeaterViewModel;

namespace DotVVM.Samples.BasicSamples.Views.ControlSamples.CheckBox
{
    public class CheckedItemsRepeaterWrapper : DotvvmMarkupControl
    {
        public Outer Data
        {
            get { return (Outer)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        public static readonly DotvvmProperty DataProperty
            = DotvvmProperty.Register<Outer, CheckedItemsRepeaterWrapper>(c => c.Data, new Outer());

        public void Update()
        {
            Data = new Outer {
                AllData = new List<Inner> {
                    new Inner {
                        Id = 1,
                        Name = "First"
                    },
                    new Inner {
                        Id = 2,
                        Name = "Second"
                    }
                },
                SelectedDataTestsIds = new List<int> { 1 }
            };
        }
    }
}
