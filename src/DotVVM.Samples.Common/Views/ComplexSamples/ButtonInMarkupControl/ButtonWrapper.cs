using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.BasicSamples.Views.ComplexSamples.ButtonInMarkupControl
{
    public class ButtonWrapper : DotvvmMarkupControl
    {
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty
            = DotvvmProperty.Register<bool, ButtonWrapper>(c => c.Enabled, true);

        public Command ClickCommand
        {
            get { return (Command)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }
        public static readonly DotvvmProperty ClickCommandProperty
            = DotvvmProperty.Register<Command, ButtonWrapper>(c => c.ClickCommand, null);
    }
}
