using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Html control, where you can configure tag name and if to render the wrapper tag by properties
    /// </summary>
    public class ConfigurableHtmlControl: HtmlGenericControl
    {
        public string WrapperTagName
        {
            get { return (string)GetValue(WrapperTagNameProperty); }
            set { SetValue(WrapperTagNameProperty, value); }
        }
        public static readonly DotvvmProperty WrapperTagNameProperty
            = DotvvmProperty.Register<string, ConfigurableHtmlControl>(c => c.WrapperTagName, "div");

        public bool RenderWrapperTag
        {
            get { return (bool)GetValue(RenderWrapperTagProperty); }
            set { SetValue(RenderWrapperTagProperty, value); }
        }
        public static readonly DotvvmProperty RenderWrapperTagProperty
            = DotvvmProperty.Register<bool, ConfigurableHtmlControl>(nameof(RenderWrapperTag), false);

        protected override bool RendersHtmlTag => RenderWrapperTag;

        public ConfigurableHtmlControl(string tagName)
            : base(tagName)
        {
            if (tagName != "div") WrapperTagName = tagName;
            if (tagName == null) RenderWrapperTag = true;
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            TagName = WrapperTagName;
        }
    }
}
