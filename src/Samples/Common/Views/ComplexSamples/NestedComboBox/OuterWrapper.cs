using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.NestedComboBox;

namespace DotVVM.Samples.BasicSamples.Views.ComplexSamples.NestedComboBox
{
    public class OuterWrapper : DotvvmMarkupControl
    {
        public InnerViewModel InnerViewModel
        {
            get { return (InnerViewModel)GetValue(InnerViewModelProperty); }
            set { SetValue(InnerViewModelProperty, value); }
        }
        public static readonly DotvvmProperty InnerViewModelProperty
            = DotvvmProperty.Register<InnerViewModel, OuterWrapper>(c => c.InnerViewModel, null);


        public int? SelectedValue
        {
            get { return (int?)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }
        public static readonly DotvvmProperty SelectedValueProperty
            = DotvvmProperty.Register<int?, OuterWrapper>(c => c.SelectedValue, null);
    }
}
