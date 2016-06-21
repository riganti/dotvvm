using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Runtime;
using Newtonsoft.Json;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A base class for all controls with Dothtml markup.
    /// </summary>
    public class DotvvmMarkupControl : HtmlGenericControl
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmMarkupControl"/> class.
        /// </summary>
        public DotvvmMarkupControl() : base("div")
        {
            SetValue(Internal.IsNamingContainerProperty, true);
            SetValue(Internal.IsControlBindingTargetProperty, true);
        }

        protected internal override void OnLoad(Hosting.IDotvvmRequestContext context)
        {
            base.OnLoad(context);
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var properties =
                GetDeclaredProperties()
                .Where(p => !p.DeclaringType.IsAssignableFrom(typeof(DotvvmMarkupControl)))
                .Select(GetPropertySerializationInfo)
                .Where(p => p.IsSerializable)
                .Select(p => JsonConvert.SerializeObject(p.Property.Name) + ": " + p.Js);

            writer.WriteKnockoutDataBindComment("dotvvm_withControlProperties", "{ " + string.Join(", ", properties) + " }");
            base.RenderContents(writer, context);
            writer.WriteKnockoutDataBindEndComment();
        }

        private PropertySerializeInfo GetPropertySerializationInfo(DotvvmProperty property)
        {
            var binding = GetBinding(property);

            if (binding == null)
            {

                return new PropertySerializeInfo
                {
                    Property = property,
                    Js = JsonConvert.SerializeObject(GetValue(property), Formatting.None),
                    IsSerializable = true
                };
            }
            else if (binding is IValueBinding)
            {
                return new PropertySerializeInfo
                {
                    Property = property,
                    Js = (binding as IValueBinding).GetKnockoutBindingExpression(),
                    IsSerializable = true

                };
            }
            else
            {
                return new PropertySerializeInfo { Property = property };
            }
        }

        private class PropertySerializeInfo
        {
            public string Js { get; set; }
            public DotvvmProperty Property { get; set; }
            public bool IsSerializable { get; set; }
        }
    }
}
