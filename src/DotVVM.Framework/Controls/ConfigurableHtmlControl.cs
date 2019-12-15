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
    /// A base class for HTML controls which allows the user to configure rendered tag name and if the wrapper tag by properties.
    /// </summary>
    public abstract class ConfigurableHtmlControl : HtmlGenericControl
    {

        /// <summary>
        /// Gets or sets the name of the tag that wraps the Repeater.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string WrapperTagName
        {
            get { return (string)GetValue(WrapperTagNameProperty); }
            set { SetValue(WrapperTagNameProperty, value); }
        }
        public static readonly DotvvmProperty WrapperTagNameProperty
            = DotvvmProperty.Register<string, ConfigurableHtmlControl>(c => c.WrapperTagName, "div");

        /// <summary>
        /// Gets or sets whether the control should render a wrapper element.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
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
            WrapperTagName = tagName;
            RenderWrapperTag = !string.IsNullOrEmpty(WrapperTagName);
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            TagName = WrapperTagName;
        }
    }
}
