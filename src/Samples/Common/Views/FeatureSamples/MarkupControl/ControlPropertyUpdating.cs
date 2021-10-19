using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.BasicSamples.Views.FeatureSamples.MarkupControl
{
    public class ControlPropertyUpdating: DotvvmMarkupControl
    {
        public string ControlProperty {
            get { return (string)GetValue(ControlPropertyProperty); }
            set { SetValue(ControlPropertyProperty, value); }
        }
        public static readonly DotvvmProperty ControlPropertyProperty =
            DotvvmProperty.Register<string, ControlPropertyUpdating>(t => t.ControlProperty, "");

        public void UpdateProperty()
        {
            SetValueToSource(ControlPropertyProperty, "ABC FFF");
        }
    }
}
