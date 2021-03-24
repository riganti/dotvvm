using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.FeatureSamples.MarkupControl
{
    public class Dialog : DotvvmMarkupControl
    {
        public Command Ok
        {
            get { return (Command)GetValue(OkProperty); }
            set { SetValue(OkProperty, value); }
        }
        public static readonly DotvvmProperty OkProperty
            = DotvvmProperty.Register<Command, Dialog>(c => c.Ok, null);

        public Command Cancel
        {
            get { return (Command)GetValue(CancelProperty); }
            set { SetValue(CancelProperty, value); }
        }
        public static readonly DotvvmProperty CancelProperty
            = DotvvmProperty.Register<Command, Dialog>(c => c.Cancel, null);
    }
}

