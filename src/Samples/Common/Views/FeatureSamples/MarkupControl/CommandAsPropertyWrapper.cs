using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.FeatureSamples.MarkupControl
{
    public class CommandAsPropertyWrapper : DotvvmMarkupControl
    {

        public Func<string, bool, Task> Click
        {
            get { return (Func<string, bool, Task>)GetValue(ClickProperty); }
            set { SetValue(ClickProperty, value); }
        }
        public static readonly DotvvmProperty ClickProperty
            = DotvvmProperty.Register<Func<string, bool, Task>, CommandAsPropertyWrapper>(c => c.Click, null);

    }
}

