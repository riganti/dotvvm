using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding;

namespace DotVVM.Samples.BasicSamples.Views.FeatureSamples.MarkupControl
{
	public class MyButton : DotvvmMarkupControl
	{
        public Command DoAction
        {
            get { return (Command)GetValue(DoActionProperty); }
            set { SetValue(DoActionProperty, value); }
        }
        public static readonly DotvvmProperty DoActionProperty
            = DotvvmProperty.Register<Command, MyButton>(c => c.DoAction, null);

    }
}

