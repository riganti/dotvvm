using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.FeatureSamples.MarkupControl
{
	public class HierarchyControl : DotvvmMarkupControl
	{
        public string PrefixText
        {
            get { return (string)GetValue(PrefixTextProperty); }
            set { SetValue(PrefixTextProperty, value); }
        }
        public static readonly DotvvmProperty PrefixTextProperty =
            DotvvmProperty.Register<string, HierarchyControl>(t => t.PrefixText, "", isValueInherited: true);

        public string NewTitle
        {
            get { return (string)GetValue(NewTitleProperty); }
            set { SetValue(NewTitleProperty, value); }
        }
        public static readonly DotvvmProperty NewTitleProperty =
            DotvvmProperty.Register<string, HierarchyControl>(t => t.NewTitle, "", isValueInherited: true);

        public void ModifyTitle(HierarchicalItem item, string newTitle)
        {
            item.Title = newTitle;
        }
    }
}

