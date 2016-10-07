using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;

namespace DotVVM.Samples.BasicSamples.Views.FeatureSamples.MarkupControl
{
	public class TextEditorControl : DotvvmMarkupControl
	{
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DotvvmProperty TextProperty
            = DotvvmProperty.Register<string, TextEditorControl>(c => c.Text, null);

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DotvvmProperty LabelProperty
            = DotvvmProperty.Register<string, TextEditorControl>(c => c.Label, null);

        public string Trap
        {
            get { return (string)GetValue(TrapProperty); }
            set { SetValue(TrapProperty, value); }
        }
        public static readonly DotvvmProperty TrapProperty
            = DotvvmProperty.Register<string, TextEditorControl>(c => c.Trap, null);
    }
}

