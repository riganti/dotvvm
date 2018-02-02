using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.FeatureSamples.MarkupControl
{
    public class ControlControlCommandInvokeAction : DotvvmMarkupControl
    {
        public ControlControlCommandInvokeAction()
        :base("span")
        {
        }
        public int Row
        {
            get { return (int)GetValue(RowProperty); }
            set { SetValue(RowProperty, value); }
        }
        public static readonly DotvvmProperty RowProperty
            = DotvvmProperty.Register<int, ControlControlCommandInvokeAction>(c => c.Row, -1);

        public int Column
        {
            get { return (int)GetValue(ColumnProperty); }
            set { SetValue(ColumnProperty, value); }
        }
        public static readonly DotvvmProperty ColumnProperty
            = DotvvmProperty.Register<int, ControlControlCommandInvokeAction>(c => c.Column, -1);


        public Action<int, int> GoToDetailAction
        {
            get { return (Action<int, int>)GetValue(GoToDetailActionProperty); }
            set { SetValue(GoToDetailActionProperty, value); }
        }
        public static readonly DotvvmProperty GoToDetailActionProperty
            = DotvvmProperty.Register<Action<int, int>, ControlControlCommandInvokeAction>(c => c.GoToDetailAction, null);

        public void OnGoToDetail()
        {
            this.GoToDetailAction?.Invoke(Row, Column);
        }
    }
}

