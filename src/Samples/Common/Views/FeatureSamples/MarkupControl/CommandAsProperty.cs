using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.FeatureSamples.MarkupControl
{
    public class CommandAsProperty : DotvvmMarkupControl
    {

        public Func<Task> Click
        {
            get => (Func<Task>)GetValue(ClickProperty)!;
            set => SetValue(ClickProperty, value);
        }
        public static readonly DotvvmProperty ClickProperty
                = DotvvmProperty.Register<Func<Task>, CommandAsProperty>(c => c.Click, null);

    }
}

