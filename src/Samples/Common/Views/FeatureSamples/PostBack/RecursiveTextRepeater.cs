using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.PostBack;

namespace DotVVM.Samples.Common.Views.FeatureSamples.PostBack
{
    public class RecursiveTextRepeater : DotvvmMarkupControl
    {
        public List<TestDataItem> Data
        {
            get => (List<TestDataItem>)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
        public static readonly DotvvmProperty DataProperty
            = DotvvmProperty.Register<List<TestDataItem>, RecursiveTextRepeater>(c => c.Data, null);

        public void ControlCommand(TestDataItem item)
        {
            item.Text = "Control command executed";
        }

    }
}

