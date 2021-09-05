using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.ControlSamples.TemplateHost
{
    public class TemplatedMarkupControl : DotvvmMarkupControl
    {

        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }
        public static readonly DotvvmProperty HeaderTextProperty
            = DotvvmProperty.Register<string, TemplatedMarkupControl>(c => c.HeaderText, null);

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement, Required = true)]
        public ITemplate ContentTemplate
        {
            get { return (ITemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }
        public static readonly DotvvmProperty ContentTemplateProperty
            = DotvvmProperty.Register<ITemplate, TemplatedMarkupControl>(c => c.ContentTemplate, null);


    }
}

