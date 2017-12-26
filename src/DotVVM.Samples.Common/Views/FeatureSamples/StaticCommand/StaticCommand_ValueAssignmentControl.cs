using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.FeatureSamples.StaticCommand
{
    public class StaticCommand_ValueAssignmentControl : DotvvmMarkupControl
    {

        public bool BoolValue
        {
            get { return (bool)GetValue(BoolValueProperty); }
            set { SetValue(BoolValueProperty, value); }
        }
        public static readonly DotvvmProperty BoolValueProperty
            = DotvvmProperty.Register<bool, StaticCommand_ValueAssignmentControl>(c => c.BoolValue, false);

        public string StringValue
        {
            get { return (string)GetValue(StringValueProperty); }
            set { SetValue(StringValueProperty, value); }
        } 
        public static readonly DotvvmProperty StringValueProperty
            = DotvvmProperty.Register<string, StaticCommand_ValueAssignmentControl>(c => c.StringValue, null);

        public int IntValue
        {
            get { return (int)GetValue(IntValueProperty); }
            set { SetValue(IntValueProperty, value); }
        }
        public static readonly DotvvmProperty IntValueProperty
            = DotvvmProperty.Register<int, StaticCommand_ValueAssignmentControl>(c => c.IntValue, 0);


    }
}

