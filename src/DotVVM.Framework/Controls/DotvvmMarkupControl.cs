#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;
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
        public DotvvmMarkupControl(string? wrapperTagName) : base(wrapperTagName)
        {
            SetValue(Internal.IsNamingContainerProperty, true);
            SetValue(Internal.IsControlBindingTargetProperty, true);
        }

        internal override void OnPreInit(IDotvvmRequestContext context)
        {
            string? wrapperTagName;

            if (Directives.ContainsKey(ParserConstants.WrapperTagNameDirective) &&
                Directives.ContainsKey(ParserConstants.NoWrapperTagNameDirective))
            {
                throw new DotvvmControlException(this, $"Control cannot have {ParserConstants.WrapperTagNameDirective} and {ParserConstants.NoWrapperTagNameDirective} at the same time");
            }

            if (Directives.TryGetValue(ParserConstants.WrapperTagNameDirective, out wrapperTagName))
            {
                TagName = wrapperTagName;
            }
            else if (Directives.ContainsKey(ParserConstants.NoWrapperTagNameDirective))
            {
                TagName = null;
            }

            foreach (var viewModule in this.GetValue<ImmutableList<ViewModuleReferenceInfo>>(Internal.ReferencedViewModuleInfoProperty)!)
            {
                Debug.Assert(viewModule.IsMarkupControl);
                context.ResourceManager.AddRequiredResource(viewModule.ImportResourceName);
            }

            base.OnPreInit(context);
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var properties =
                GetDeclaredProperties()
                .Where(p => !p.DeclaringType.IsAssignableFrom(typeof(DotvvmMarkupControl)))
                .Select(GetPropertySerializationInfo)
                .Where(p => p.Js is object)
                .Select(p => JsonConvert.ToString(p.Property.Name, '"', StringEscapeHandling.EscapeHtml) + ": " + p.Js);

            var viewModule =
                this.GetValue<ImmutableList<ViewModuleReferenceInfo>>(Internal.ReferencedViewModuleInfoProperty)
                .SingleOrDefault(); // Control can only contain one reference info - it can not have master pages

            writer.WriteKnockoutDataBindComment("dotvvm-with-control-properties", "{ " + string.Join(", ", properties) + " }");
            if (viewModule is object)
            {
                var viewId = this.GetDotvvmUniqueId();
                var viewIdJs = (viewId as IValueBinding)?.GetKnockoutBindingExpression(this) ??
                               JsonConvert.ToString((string)viewId, '"', StringEscapeHandling.EscapeHtml);
                var settings = DefaultSerializerSettingsProvider.Instance.GetSettingsCopy();
                settings.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

                writer.WriteKnockoutDataBindComment("dotvvm-with-view-modules",
                    $"{{ viewId: {viewIdJs}, modules: {JsonConvert.SerializeObject(viewModule.ReferencedModules, settings)} }}"
                );
            }
            base.RenderContents(writer, context);
            writer.WriteKnockoutDataBindEndComment();
            if (viewModule is object)
            {
                writer.WriteKnockoutDataBindEndComment();
            }
        }

        private PropertySerializeInfo GetPropertySerializationInfo(DotvvmProperty property)
        {
            if (ContainsPropertyStaticValue(property))
            {
                var settings = DefaultSerializerSettingsProvider.Instance.GetSettingsCopy();
                settings.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

                return new PropertySerializeInfo(
                    property,
                    JsonConvert.SerializeObject(GetValue(property), Formatting.None, settings)
                );
            }
            else if (GetBinding(property) is IValueBinding valueBinding)
            {
                return new PropertySerializeInfo(
                    property,
                    valueBinding.GetKnockoutBindingExpression(this)
                );
            }
            else
            {
                return new PropertySerializeInfo(property, null);
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
            public PropertySerializeInfo(DotvvmProperty property, string? js)
            {
                this.Js = js;
                this.Property = property;
            }
            public string? Js { get; set; }
            public DotvvmProperty Property { get; set; }
        }
    }
}
