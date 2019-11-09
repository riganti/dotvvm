#nullable enable
using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    [ControlMarkupOptions(AllowContent = false)]
    public class SelectorItem : HtmlGenericControl
    {
        public string? Text
        {
            get { return (string?)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<string?, SelectorItem>(t => t.Text, null);

        public object? Value
        {
            get { return (object?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DotvvmProperty ValueProperty =
            DotvvmProperty.Register<object?, SelectorItem>(t => t.Value, null);

        public SelectorItem()
            : base("option")
        {
        }

        public SelectorItem(string text, object value)
            : this()
        {
            Text = text;
            Value = value;
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddAttribute("value", Value + "");
            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (Text is string t)
                writer.WriteText(t);
            base.RenderContents(writer, context);
        }
    }
}
