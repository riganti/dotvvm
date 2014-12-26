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



        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly RedwoodProperty ValueProperty =
            RedwoodProperty.RegisterControlStateProperty<int, Sample5_TestControl>(c => c.Value);


        public override bool RequiresControlState
        {
            get { return true; }
        }
    }
}