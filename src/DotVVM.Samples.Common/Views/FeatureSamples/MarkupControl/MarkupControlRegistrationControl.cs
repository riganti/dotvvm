using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.BasicSamples.Views.FeatureSamples.MarkupControl
{
    public class MarkupControlRegistrationControl : DotvvmMarkupControl
    {

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly DotvvmProperty NameProperty =
            DotvvmProperty.Register<string, MarkupControlRegistrationControl>(c => c.Name, string.Empty);



        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DotvvmProperty ValueProperty =
            DotvvmProperty.Register<int, MarkupControlRegistrationControl>(c => c.Value, 0);


        public void Up()
        {
            Value++;
        }

        public void Down()
        {
            Value--;
        }
    }
}

