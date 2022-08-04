using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmMarkupControl"/> class.
        /// </summary>
        public DotvvmMarkupControl(string? wrapperTagName) : base(wrapperTagName, false)
        {
            LifecycleRequirements |= ControlLifecycleRequirements.PreInit;
            SetValue(Internal.IsNamingContainerProperty, true);
            SetValue(Internal.IsControlBindingTargetProperty, true);
        }

        internal override void OnPreInit(IDotvvmRequestContext context)
        {
            if (Directives.ContainsKey(ParserConstants.WrapperTagNameDirective) && Directives.ContainsKey(ParserConstants.NoWrapperTagNameDirective))
            {
                throw new DotvvmControlException(this, $"Control cannot have {ParserConstants.WrapperTagNameDirective} and {ParserConstants.NoWrapperTagNameDirective} at the same time");
            }

            if (Directives.TryGetValue(ParserConstants.WrapperTagNameDirective, out var wrapperTagName))
            {
                TagName = wrapperTagName;
            }
            else if (Directives.ContainsKey(ParserConstants.NoWrapperTagNameDirective))
            {
                TagName = null;
            }

            var viewModule = GetValue<ViewModuleReferenceInfo>(Internal.ReferencedViewModuleInfoProperty);
            if (viewModule is object)
            {
                Debug.Assert(viewModule.IsMarkupControl);
                context.ResourceManager.AddRequiredResource(viewModule.ImportResourceName);
            }

            base.OnPreInit(context);
        }

        int knockoutCommentsToEnd = 0;

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var properties = new KnockoutBindingGroup();
            var usedProperties = GetValue<ControlUsedPropertiesInfo>(Internal.UsedPropertiesInfoProperty);
            foreach (var p in usedProperties?.ClientSideUsedProperties ?? GetDeclaredProperties())
            {
                if (p.DeclaringType.IsAssignableFrom(typeof(DotvvmMarkupControl)))
                    continue;

                var pinfo = GetPropertySerializationInfo(p); // migrate to use the KnockoutBindingGroup helpers
                if (pinfo.Js is object)
                {
                    properties.Add(p.Name, pinfo.Js);
                }
            }

            foreach (var pg in usedProperties?.ClientSideUsedPropertyGroups ?? DotvvmPropertyGroup.GetPropertyGroups(this.GetType()))
            {
                if (pg.DeclaringType.IsAssignableFrom(typeof(DotvvmMarkupControl)))
                    continue;

                var js = new StringBuilder().Append("[");
                
                var values = new VirtualPropertyGroupDictionary<object>(this, pg);
                foreach (var p in values.Properties.OrderBy(p => p.GroupMemberName))
                {
                    var pinfo = GetPropertySerializationInfo(p); // migrate to use the KnockoutBindingGroup helpers
                    if (pinfo.Js is object)
                    {
                        js.Append("{Key: ")
                          .Append(JsonConvert.ToString(p.GroupMemberName, '"', StringEscapeHandling.EscapeHtml))
                          .Append(", Value: ")
                          .Append(pinfo.Js)
                          .Append("},");
                    }
                }
                js.Append("]");
                properties.Add(pg.Name, js.ToString());
            }

            if (!properties.IsEmpty)
            {
                if (RendersHtmlTag)
                    writer.AddKnockoutDataBind("dotvvm-with-control-properties", properties);
                else
                {
                    writer.WriteKnockoutDataBindComment("dotvvm-with-control-properties", properties.ToString());
                    knockoutCommentsToEnd++;
                }
            }

            var viewModule = this.GetValue<ViewModuleReferenceInfo>(Internal.ReferencedViewModuleInfoProperty);
            if (viewModule is object)
            {
                var settings = DefaultSerializerSettingsProvider.Instance.GetSettingsCopy();
                settings.StringEscapeHandling = StringEscapeHandling.EscapeHtml;
                var binding = $"{{ modules: {JsonConvert.SerializeObject(viewModule.ReferencedModules, settings)} }}";
                if (RendersHtmlTag)
                    writer.AddKnockoutDataBind("dotvvm-with-view-modules", binding);
                else
                {
                    writer.WriteKnockoutDataBindComment("dotvvm-with-view-modules", binding);
                    knockoutCommentsToEnd++;
                }
            }


            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.RenderEndTag(writer, context);

            while (knockoutCommentsToEnd > 0)
            {
                writer.WriteKnockoutDataBindEndComment();
                knockoutCommentsToEnd--;
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
            else if (GetBinding(property) is ICommandBinding command)
            {

                return new PropertySerializeInfo(
                    property,
                    KnockoutHelper.GenerateClientPostbackLambda(
                        property.Name,
                        command,
                        this
                    )
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
                Js = js;
                Property = property;
            }
            public string? Js { get; set; }
            public DotvvmProperty Property { get; set; }
        }
    }
}
