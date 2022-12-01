using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.FeatureSamples.ViewModules
{
    public class LinkModuleControl : DotvvmMarkupControl
    {
        [PropertyGroup("Query-")]
        public VirtualPropertyGroupDictionary<object> QueryParameters => new VirtualPropertyGroupDictionary<object>(this, QueryParametersGroupDescriptor);
        public static DotvvmPropertyGroup QueryParametersGroupDescriptor =
            DotvvmPropertyGroup.Register<object, LinkModuleControl>("Query-", "QueryParameters");

        public string Page
        {
            get { return (string)GetValue(PageProperty); }
            set { SetValue(PageProperty, value); }
        }
        public static readonly DotvvmProperty PageProperty
            = DotvvmProperty.Register<string, LinkModuleControl>(c => c.Page, null);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DotvvmProperty TextProperty
            = DotvvmProperty.Register<string, LinkModuleControl>(c => c.Text, null);
    }
}
