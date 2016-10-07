using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Samples.BasicSamples.Views.ComplexSamples.ServerRendering
{
	public class ArticleEditor : ArticleBase
	{
        [MarkupOptions(Required = true)]
        public DateTime Date
        {
            get { return (DateTime)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }
        public static readonly DotvvmProperty DateProperty
            = DotvvmProperty.Register<DateTime, ArticleEditor>(c => c.Date, default(DateTime));
    }
}

