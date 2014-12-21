using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls;

namespace Redwood.Samples.BasicSamples.Controls
{
    public class Sample5_TestControl : RedwoodMarkupControl
    {

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly RedwoodProperty NameProperty =
            RedwoodProperty.Register<string, Sample5_TestControl>(c => c.Name, string.Empty);

    }
}