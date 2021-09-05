using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using System;

namespace DotVVM.Samples.Common.Views.ControlSamples.Repeater.SampleControl
{
    public class ControlWithButton : DotvvmMarkupControl
    {
        public Action GoToDetailAction
        {
            get { return (Action)GetValue(GoToDetailActionProperty); }
            set { SetValue(GoToDetailActionProperty, value); }
        }
        public static readonly DotvvmProperty GoToDetailActionProperty
            = DotvvmProperty.Register<Action, ControlWithButton>(c => c.GoToDetailAction, null);


        public void OnGoToDetail()
        {
            this.GoToDetailAction?.Invoke();
        }
    }
}
