using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.Views.ComplexSamples.ServerRendering
{
    public class ArticleBase : DotvvmMarkupControl
    {
        [MarkupOptions(Required = true)]
        public string OriginalMessage
        {
            get { return (string)GetValue(OriginalMessageProperty); }
            set { SetValue(OriginalMessageProperty, value); }
        }
        public static readonly DotvvmProperty OriginalMessageProperty
            = DotvvmProperty.Register<string, ArticleBase>(c => c.OriginalMessage, "");
    }
}
