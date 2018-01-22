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

        public StaticCommand_ValueAssignmentControlModel Model
        {
            get { return (StaticCommand_ValueAssignmentControlModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }
        public static readonly DotvvmProperty ModelProperty
            = DotvvmProperty.Register<StaticCommand_ValueAssignmentControlModel, StaticCommand_ValueAssignmentControl>(c => c.Model, null);
    }


    public class StaticCommand_ValueAssignmentControlModel
    {

        public string Value { get; set; }
    }

}

