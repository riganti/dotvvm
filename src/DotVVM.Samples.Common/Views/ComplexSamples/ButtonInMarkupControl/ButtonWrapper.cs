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

        public Command OnCommand
        {
            get { return (Command)GetValue(OnCommandProperty); }
            set { SetValue(OnCommandProperty, value); }
        }
        public static readonly DotvvmProperty OnCommandProperty
            = DotvvmProperty.Register<Command, ButtonWrapper>(c => c.OnCommand, null);

        public Command OffCommand
        {
            get { return (Command)GetValue(OffCommandProperty); }
            set { SetValue(OffCommandProperty, value); }
        }
        public static readonly DotvvmProperty OffCommandProperty
            = DotvvmProperty.Register<Command, ButtonWrapper>(c => c.OffCommand, null);
    }
}
