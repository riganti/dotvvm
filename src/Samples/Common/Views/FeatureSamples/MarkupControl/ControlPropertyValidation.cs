using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;

namespace DotVVM.Samples.BasicSamples.Views.FeatureSamples.MarkupControl
{
    public class ControlPropertyValidation : DotvvmMarkupControl
    {
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DotvvmProperty ValueProperty
            = DotvvmProperty.Register<string, ControlPropertyValidation>(c => c.Value, null);

    }
}

