using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.FeatureSamples.MarkupControl
{
    public class Device : DotvvmMarkupControl
    {
        public Command Edit
        {
            get => (Command)GetValue(EditProperty);
            set => SetValue(EditProperty, value);
        }
        public static readonly DotvvmProperty EditProperty
            = DotvvmProperty.Register<Command, Device>(c => c.Edit, null);

        public Command Remove
        {
            get => (Command)GetValue(RemoveProperty);
            set => SetValue(RemoveProperty, value);
        }
        public static readonly DotvvmProperty RemoveProperty
            = DotvvmProperty.Register<Command, Device>(c => c.Remove, null);

        public string MyProperty
        {
            get { return (string)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }
        public static readonly DotvvmProperty MyPropertyProperty
            = DotvvmProperty.Register<string, Device>(c => c.MyProperty, null);


    }
}
