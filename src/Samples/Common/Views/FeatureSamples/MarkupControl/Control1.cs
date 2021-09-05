using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.FeatureSamples.MarkupControl
{
	public class Control1 : DotvvmMarkupControl
	{
        public string MyProperty
        {
            get { return (string)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }
        public static readonly DotvvmProperty MyPropertyProperty
            = DotvvmProperty.Register<string, Control1>(c => c.MyProperty, null);

    }
}

