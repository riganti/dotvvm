using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.NestedComboBox;

namespace DotVVM.Samples.BasicSamples.Views.ComplexSamples.NestedComboBox
{
    public class InnerWrapper : DotvvmMarkupControl
    {
        public int? SelectedValue
        {
            get { return (int?)GetValue(SelectedValueProperty); }
            set { SetValueToSource(SelectedValueProperty, value); }
        }
        public static readonly DotvvmProperty SelectedValueProperty
            = DotvvmProperty.Register<int?, InnerWrapper>(c => c.SelectedValue, null);

        public void Change()
        {
            if (SelectedValue <= 1)
            {
                SelectedValue = null;
            }
        }
    }
}
