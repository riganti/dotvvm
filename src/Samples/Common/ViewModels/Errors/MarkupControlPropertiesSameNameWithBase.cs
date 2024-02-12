using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.ViewModels.Errors
{
    public class MarkupControlPropertiesSameNameWithBase : DotvvmMarkupControl
    {
        public string MyProperty
        {
            get { return (string)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }
        public static readonly DotvvmProperty MyPropertyProperty
            = DotvvmProperty.Register<string, MarkupControlPropertiesSameNameWithBase>(c => c.MyProperty, null);

    }
}
