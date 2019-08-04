using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Serialization;
using Newtonsoft.Json;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A base class for all controls with Dothtml markup.
    /// </summary>
    public class DotvvmMarkupControl : HtmlGenericControl
    {
        public Dictionary<string, string> Directives { get; } = new Dictionary<string, string>();

        private bool rendersWrapperTag;
        protected override bool RendersHtmlTag => rendersWrapperTag;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmMarkupControl"/> class.
        /// </summary>
        public DotvvmMarkupControl() : this("div")
        {
            LifecycleRequirements |= ControlLifecycleRequirements.PreInit;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmMarkupControl"/> class.
        /// </summary>
        public DotvvmMarkupControl(string wrapperTagName) : base(wrapperTagName ?? "JUST NOTHING")
        {
            SetValue(Internal.IsNamingContainerProperty, true);
            SetValue(Internal.IsControlBindingTargetProperty, true);
            rendersWrapperTag = wrapperTagName != null;
        }

        internal override void OnPreInit(IDotvvmRequestContext context)
        {
            string wrapperTagName;

            if (Directives.ContainsKey(ParserConstants.WrapperTagNameDirective) &&
                Directives.ContainsKey(ParserConstants.NoWrapperTagNameDirective))
            {
                throw new DotvvmControlException(this, $"Control cannot have {ParserConstants.WrapperTagNameDirective} and {ParserConstants.NoWrapperTagNameDirective} at the same time");
            }

            if (Directives.TryGetValue(ParserConstants.WrapperTagNameDirective, out wrapperTagName))
            {
                rendersWrapperTag = true;
                TagName = wrapperTagName;
            }
            if (Directives.ContainsKey(ParserConstants.NoWrapperTagNameDirective))
            {
                rendersWrapperTag = false;
            }
            base.OnPreInit(context);
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
                .Select(p => JsonConvert.ToString(p.Property.Name, '"', StringEscapeHandling.EscapeHtml) + ": " + p.Js);

            writer.WriteKnockoutDataBindComment("dotvvm_withControlProperties", "{ " + string.Join(", ", properties) + " }");
            base.RenderContents(writer, context);
            writer.WriteKnockoutDataBindEndComment();
        }

        private PropertySerializeInfo GetPropertySerializationInfo(DotvvmProperty property)
        {
            if (ContainsPropertyStaticValue(property))
            {
                JsonSerializerSettings settings = DefaultViewModelSerializer.CreateDefaultSettings();
                settings.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

                return new PropertySerializeInfo {
                    Property = property,
                    Js = JsonConvert.SerializeObject(GetValue(property), Formatting.None, settings),
                    IsSerializable = true
                };
            }
            else if (GetBinding(property) is IValueBinding valueBinding)
            {
                return new PropertySerializeInfo {
                    Property = property,
                    Js = valueBinding.GetKnockoutBindingExpression(this),
                    IsSerializable = true
                };
            }
            else
            {
                return new PropertySerializeInfo { Property = property };
            }
        }

        private bool ContainsPropertyStaticValue(DotvvmProperty property)
        {
            var binding = GetBinding(property);

            var isValueOrServerSideValueBinding = binding == null || (binding is IStaticValueBinding && !(binding is IValueBinding));

            return isValueOrServerSideValueBinding && !typeof(ITemplate).IsAssignableFrom(property.PropertyType);
        }

        private class PropertySerializeInfo
        {
            public string Js { get; set; }
            public DotvvmProperty Property { get; set; }
            public bool IsSerializable { get; set; }
        }
    }
}
