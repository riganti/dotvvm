using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class Sample5_TestControl : DotvvmMarkupControl
    {

        public string Name
        {
            get { return (string)GetValue(NameProperty); } 
            set { SetValue(NameProperty, value); }
        }

        public static readonly DotvvmProperty NameProperty = 
            DotvvmProperty.Register<string, Sample5_TestControl>(c => c.Name, string.Empty);



        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DotvvmProperty ValueProperty =
            DotvvmProperty.RegisterControlStateProperty<int, Sample5_TestControl>(c => c.Value, 0);


        protected override bool RequiresControlState
        {
            get { return true; }
        }




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