using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Runtime;
using Newtonsoft.Json;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A base class for all controls with Dothtml markup.
    /// </summary>
    public abstract class DotvvmMarkupControl : HtmlGenericControl
    {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmMarkupControl"/> class.
        /// </summary>
        public DotvvmMarkupControl() : base("div")
        {
            SetValue(Internal.IsNamingContainerProperty, true);
            SetValue(Internal.IsControlBindingTargetProperty, true);
        }

        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            EnsureControlHasId();
            base.OnLoad(context);
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            var properties = this.properties.Keys.Where(p => GetType().IsAssignableFrom(p.DeclaringType))
                .Select(p => new
                {
                    js = GetJsValue(p),
                    property = p
                })
                .Select(p => JsonConvert.SerializeObject(p.property.Name) + ": " + p.js);

            writer.WriteKnockoutDataBindComment("withControlProperties", "{ " + string.Join(", ", properties) + " }");
            base.RenderContents(writer, context);
            writer.WriteKnockoutDataBindEndComment();
        }

        private string GetJsValue(DotvvmProperty property)
        {
            var binding = GetBinding(property);
            if (binding?.Javascript != null)
            {
                return binding.Javascript;
            }
            return JsonConvert.SerializeObject(GetValue(property), Formatting.None);
        }
    }
}
