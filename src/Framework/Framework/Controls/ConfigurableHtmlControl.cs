using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Validation;
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
        public string? WrapperTagName
        {
            get { return (string?)GetValue(WrapperTagNameProperty); }
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
            get { return (bool)GetValue(RenderWrapperTagProperty)!; }
            set { SetValue(RenderWrapperTagProperty, value); }
        }
        public static readonly DotvvmProperty RenderWrapperTagProperty
            = DotvvmProperty.Register<bool, ConfigurableHtmlControl>(nameof(RenderWrapperTag), false);

        protected override bool RendersHtmlTag => RenderWrapperTag;

        public ConfigurableHtmlControl(string? tagName)
            : base(tagName, false)
        {
            WrapperTagName = tagName;
            RenderWrapperTag = !string.IsNullOrEmpty(tagName);
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            TagName = WrapperTagName;
        }

        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            var type = control.Metadata.Type;
            // control can set the default value of RenderWrapperTag in constructor, so we have no way to know it in general
            // However, we know about some built-in controls
            var defaultValue =
                type == typeof(HtmlLiteral) ? true :
                type == typeof(SpaContentPlaceHolder) ? true :
                type == typeof(ContentPlaceHolder) ? false :
                type == typeof(ClaimView) ? false :
                type == typeof(EnvironmentView) ? false :
                type == typeof(RoleView) ? false :
                type == typeof(AuthenticatedView) ? false :
                (bool?)null;
            var hasDefaultTagName =
                type != typeof(ContentPlaceHolder);
            var renderWrapperTag = (control.GetValue(RenderWrapperTagProperty) as ResolvedPropertyValue)?.Value as bool? ?? defaultValue;
            var wrapperTagName = (control.GetValue(WrapperTagNameProperty) as ResolvedPropertyValue)?.Value as string;

            if (wrapperTagName?.Trim() == "")
            {
                yield return new ControlUsageError("The WrapperTagName must not be an empty string!");
            }
            else if (renderWrapperTag == true && (wrapperTagName == null) && !hasDefaultTagName)
            {
                yield return new ControlUsageError("The WrapperTagName property must be set when RenderWrapperTag is true!");
            }
            else if (renderWrapperTag == false && (wrapperTagName != null))
            {
                yield return new ControlUsageError("The WrapperTagName property cannot be set when RenderWrapperTag is false!");
            }
        }
    }
}
