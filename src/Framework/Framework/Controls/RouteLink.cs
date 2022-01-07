using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Hyperlink which builds the URL from route name and parameter values.
    /// </summary>
    public class RouteLink : HtmlGenericControl
    {
        /// <summary>
        /// Gets or sets the name of the route in the route table.
        /// </summary>
        [MarkupOptions(AllowBinding = false, Required = true)]
        public string RouteName
        {
            get { return (string)GetValue(RouteNameProperty)!; }
            set { SetValue(RouteNameProperty, value); }
        }
        public static readonly DotvvmProperty RouteNameProperty =
            DotvvmProperty.Register<string, RouteLink>(c => c.RouteName);

        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty)!; }
            set { SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty =
            DotvvmProperty.Register<bool, RouteLink>(t => t.Enabled, true);

        /// <summary>
        /// Gets or sets the suffix that will be appended to the generated URL (e.g. query string or URL fragment).
        /// </summary>
        public string? UrlSuffix
        {
            get { return (string?)GetValue(UrlSuffixProperty); }
            set { SetValue(UrlSuffixProperty, value); }
        }
        public static readonly DotvvmProperty UrlSuffixProperty
            = DotvvmProperty.Register<string?, RouteLink>(c => c.UrlSuffix, null);


        /// <summary>
        /// Gets or sets the text of the hyperlink.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty)!; }
            set { SetValue(TextProperty, value ?? throw new ArgumentNullException(nameof(value))); }
        }
        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<string, RouteLink>(c => c.Text, "");

        [PropertyGroup("Param-")]
        public VirtualPropertyGroupDictionary<object> Params => new VirtualPropertyGroupDictionary<object>(this, ParamsGroupDescriptor);
        public static DotvvmPropertyGroup ParamsGroupDescriptor =
            DotvvmPropertyGroup.Register<object, RouteLink>("Param-", "Params");

        [PropertyGroup("Query-")]
        public VirtualPropertyGroupDictionary<object> QueryParameters => new VirtualPropertyGroupDictionary<object>(this, QueryParametersGroupDescriptor);
        public static DotvvmPropertyGroup QueryParametersGroupDescriptor =
            DotvvmPropertyGroup.Register<object, RouteLink>("Query-", "QueryParameters");

        public TextOrContentCapability TextOrContentCapability
        {
            get => (TextOrContentCapability)TextOrContentCapabilityProperty.GetValue(this);
            set => TextOrContentCapabilityProperty.SetValue(this, value);
        }
        public static readonly DotvvmCapabilityProperty TextOrContentCapabilityProperty =
            DotvvmCapabilityProperty.RegisterCapability<TextOrContentCapability, RouteLink>(
                control => TextOrContentCapability.FromChildren(control, TextProperty),
                (control, value) => {
                    value.WriteToChildren((DotvvmControl)control, TextProperty);
                }
            );

        public RouteLink() : base("a", false)
        {
        }


        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            RouteLinkHelpers.WriteRouteLinkHrefAttribute(this, writer, context);


            var textBinding = GetValueBinding(TextProperty);
            if (textBinding is object)
            {
                writer.AddKnockoutDataBind("text", textBinding, this);
            }

            var enabledBinding = GetValueBinding(EnabledProperty);

            if (enabledBinding is object)
            {
                WriteEnabledBinding(writer, enabledBinding);
            }

            if (GetValue<bool?>(EnabledProperty) == false)
            {
                writer.AddAttribute("disabled", "disabled");

                if (enabledBinding is null)
                    WriteEnabledBinding(writer, false);
            }

            WriteOnClickAttribute(writer, context);

            base.AddAttributesToRender(writer, context);
        }

        protected virtual void WriteEnabledBinding(IHtmlWriter writer, bool binding)
        {
            writer.AddKnockoutDataBind("dotvvm-enable", binding.ToString().ToLowerInvariant());
        }

        protected virtual void WriteEnabledBinding(IHtmlWriter writer, IValueBinding binding)
        {
            writer.AddKnockoutDataBind("dotvvm-enable", binding.GetKnockoutBindingExpression(this));
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var hasChildren = !HasOnlyWhiteSpaceContent();
            if (hasChildren && (HasBinding(TextProperty) || !string.IsNullOrEmpty(Text)))
            {
                throw new DotvvmControlException(this, "Text property and inner content of the <dot:RouteLink> control cannot be set at the same time!");
            }
            else if (hasChildren)
            {
                base.RenderContents(writer, context);
            }
            else
            {
                writer.WriteText(Text);
            }
        }

        protected virtual void WriteOnClickAttribute(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // a hack that makes the RouteLink work even in container with Events.Click. This does not solve the problem in general, but better than nothing.
            var onclickAttribute = "event.stopPropagation();";
            if ((bool)GetValue(Internal.IsSpaPageProperty)! && (bool)GetValue(Internal.UseHistoryApiSpaNavigationProperty)! && !Attributes.ContainsKey("target"))
            {
                onclickAttribute += "!this.hasAttribute('disabled') && dotvvm.handleSpaNavigation(this); return false;";
            }
            else
            {
                onclickAttribute += "return !this.hasAttribute('disabled');";
            }
            writer.AddAttribute("onclick", onclickAttribute);
        }

        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control, DotvvmConfiguration configuration)
        {
            var routeNameProperty = control.GetValue(RouteNameProperty);
            if (routeNameProperty is not ResolvedPropertyValue { Value: string routeName })
                yield break;

            if (!configuration.RouteTable.Contains(routeName))
            {
                yield return new ControlUsageError(
                    $"RouteName \"{routeName}\" does not exist.",
                    routeNameProperty.DothtmlNode);
                yield break;
            }

            var parameterDefinitions = configuration.RouteTable[routeName].ParameterNames;
            var parameterReferences = control.Properties.Where(i => i.Key is GroupedDotvvmProperty p && p.PropertyGroup == ParamsGroupDescriptor);

            var undefinedReferences =
                from parameterReference in parameterReferences
                let parameterGroupName = parameterReference.Value.Property.CastTo<GroupedDotvvmProperty>()?.GroupMemberName
                let parameterNode = parameterReference.Value.DothtmlNode
                where parameterGroupName is string && !parameterDefinitions.Contains(parameterGroupName, StringComparer.OrdinalIgnoreCase)
                select (parameterGroupName, parameterNode);

            if (undefinedReferences.Any())
            {
                var parameters = string.Join(", ", undefinedReferences.Select(reference => reference.parameterGroupName));
                yield return new ControlUsageError(
                    $"The following parameters are not present in route {routeName}: {parameters}",
                    undefinedReferences.Select(reference => reference.parameterNode));
            }
        }
    }
}
