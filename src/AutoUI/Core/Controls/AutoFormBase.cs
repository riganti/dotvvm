using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotVVM.AutoUI.Metadata;
using DotVVM.AutoUI.PropertyHandlers;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Validator = DotVVM.Framework.Controls.Validator;

namespace DotVVM.AutoUI.Controls
{
    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.InServerSideStyles)]
    public abstract class AutoFormBase : CompositeControl 
    {
        protected readonly IServiceProvider services;
        public AutoFormBase(IServiceProvider services)
        {
            this.services = services;
        }


        /// <summary>
        /// Gets or sets the view name (e.g. Insert, Edit, ReadOnly). Some fields may have different metadata for each view.
        /// </summary>
        public string? ViewName
        {
            get { return (string?)GetValue(ViewNameProperty); }
            set { SetValue(ViewNameProperty, value); }
        }
        public static readonly DotvvmProperty ViewNameProperty
            = DotvvmProperty.Register<string, AutoFormBase>(c => c.ViewName, null);


        /// <summary>
        /// Gets or sets the group of fields that should be rendered. If not set, fields from all groups will be rendered.
        /// </summary>
        public string? GroupName
        {
            get { return (string?)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }
        public static readonly DotvvmProperty GroupNameProperty
            = DotvvmProperty.Register<string, AutoFormBase>(c => c.GroupName, null);

        public static readonly DotvvmCapabilityProperty FieldPropsProperty =
            DotvvmCapabilityProperty.RegisterCapability<FieldProps, AutoFormBase>();

        protected AutoUIContext CreateAutoUiContext()
        {
            return new AutoUIContext(this.GetDataContextType().NotNull(), services) {
                ViewName = ViewName,
                GroupName = GroupName
            };
        }

        /// <summary>
        /// Gets the list of properties that should be displayed.
        /// </summary>
        internal static PropertyDisplayMetadata[] GetPropertiesToDisplay(AutoUIContext context, FieldSelectorProps props)
        {
            var entityPropertyListProvider = context.Services.GetRequiredService<IEntityPropertyListProvider>();
            var properties = entityPropertyListProvider.GetProperties(context.EntityType, context.CreateViewContext());

            if (props.ExcludeProperties is { })
            {
                properties = properties.Where(p => !props.ExcludeProperties.Contains(p.Name)).ToArray();
            }

            var localProperties = props.Property.Select(p => MapLocalProperty(p.Key, props, context)!).ToDictionary(p => p.Name);
            properties = properties.Select(p => {
                if (localProperties.TryGetValue(p.Name, out var localProperty))
                {
                    localProperties.Remove(p.Name);
                    if (localProperty.PropertyInfo is null)
                        return p with { ValueBinding = localProperty.ValueBinding, Type = localProperty.Type, IsEditable = p.IsEditable && localProperty.IsEditable };
                    return localProperty;
                }
                return p;
            }).Concat(
                localProperties.Values
            ).ToArray();


            if (props.IncludeProperties is { Length: > 0 })
            {
                if (props.ExcludeProperties is { Length: > 0 })
                    throw new NotSupportedException("Only one of IncludeProperties and ExcludeProperties can be specified.");

                return props.IncludeProperties.Select(prop =>
                    properties.FirstOrDefault(p => p.Name == prop)
                        ?? throw new Exception($"Property {prop} not found on entity type {context.EntityType}.")
                ).ToArray();
            }

            if (!string.IsNullOrEmpty(context.GroupName))
            {
                return properties.Where(p => p.GroupName == context.GroupName || props.Property.ContainsKey(p.Name)).ToArray();
            }
            return properties.ToArray();
        }

        protected virtual string GetEditorId(PropertyDisplayMetadata property) => property.Name + "__input";

        /// <summary>
        /// Creates the contents of the label cell for the specified property.
        /// </summary>
        protected virtual Label? InitializeControlLabel(PropertyDisplayMetadata property, AutoUIContext autoUiContext, FieldProps props)
        {
            var id = GetEditorId(property);
            if (props.Label.ContainsKey(property.Name))
            {
                return new Label(id).AppendChildren(new Literal(props.Label[property.Name]));
            }

            if (property.IsDefaultLabelAllowed)
            {
                return new Label(id).AppendChildren(new Literal(property.GetDisplayName().ToBinding(autoUiContext)));
            }
            return null;
        }

        protected virtual DotvvmControl? TryGetFieldTemplate(PropertyDisplayMetadata property, FieldProps props) =>
            props.FieldTemplate.TryGetValue(property.Name, out var template) ?
                new TemplateHost(template) : null;

        protected virtual DotvvmControl CreateEditor(PropertyDisplayMetadata property, AutoUIContext autoUiContext, FieldProps props)
        {
            var name = property.Name;

            return AutoEditor.Build(new AutoEditor.Props()
                {
                    Changed = props.Changed.GetValueOrDefault(name),
                    Enabled = props.Enabled.GetValueOrDefault(name,
                        GetEnabledResourceBinding(property, autoUiContext)),
                    OverrideTemplate = props.EditorTemplate.GetValueOrDefault(name),
                    Property = autoUiContext.CreateValueBinding(property),
                    Html = new HtmlCapability()
                    {
                        ID = ValueOrBinding<string?>.FromBoxedValue(GetEditorId(property))
                    }
                },
                autoUiContext);
        }

        protected virtual void InitializeValidation(HtmlGenericControl validatedElement, HtmlGenericControl labelElement, PropertyDisplayMetadata property, AutoUIContext context)
        {
            if (property.PropertyInfo is { } &&
                context.ValidationMetadataProvider.GetAttributesForProperty(property.PropertyInfo).OfType<RequiredAttribute>().Any())
            {
                labelElement.AddCssClass("autoui-required");
            }

            validatedElement.SetValue(Validator.ValueProperty, context.CreateValueBinding(property));
        }

        protected static PropertyDisplayMetadata? MapLocalProperty(string name, FieldSelectorProps props, AutoUIContext context)
        {
            if (!props.Property.TryGetValue(name, out var binding))
                return null;

            var property = binding.GetProperty<ReferencedViewModelPropertiesBindingProperty>()?.MainProperty;

            var isEditable = binding.GetProperty<BindingUpdateDelegate>(ErrorHandlingMode.ReturnNull) is { };

            var metadata =
                property is not null ? context.PropertyDisplayMetadataProvider.GetPropertyMetadata(property)
                                     : new PropertyDisplayMetadata(name, binding) { IsEditable = isEditable };
            return metadata with { Name = name, ValueBinding = binding };
        }

        protected virtual ValueOrBinding<bool> GetVisibleResourceBinding(PropertyDisplayMetadata metadata, AutoUIContext context)
        {
            return ConditionalFieldBindingProvider.GetPropertyBinding(metadata.VisibleAttributes, context);
        }

        protected virtual ValueOrBinding<bool> GetEnabledResourceBinding(PropertyDisplayMetadata metadata, AutoUIContext context) =>
            metadata.IsEnabledBinding(context);

        protected virtual void SetFieldVisibility(HtmlGenericControl field, PropertyDisplayMetadata property, FieldProps props, AutoUIContext context)
        {
            var visibleResourceBinding = GetVisibleResourceBinding(property, context);
            if (props.Visible.TryGetValue(property.Name, out var visible))
            {
                if (visible.BindingOrDefault is IValueBinding)
                {
                    field.SetValueRaw(HtmlGenericControl.VisibleProperty, visible.BindingOrDefault);
                }
                else
                {
                    // static values and resource bindings should be in IncludeInPage to avoid sending then to the client
                    visibleResourceBinding = visibleResourceBinding.And(visible);
                }
            }
            else if (property.SelectionConfiguration is { } selector)
            {
                try
                {
                    var dataSource = SelectorHelper.DiscoverSelectorDataSourceBinding(context, selector.PropertyType);
                    var nonEmptyBinding =
                        dataSource.GetProperty<DataSourceLengthBinding>().Binding.GetProperty<IsMoreThanZeroBindingProperty>().Binding;
                    field.SetValueRaw(HtmlGenericControl.VisibleProperty, nonEmptyBinding);
                }
                catch
                {
                    // nvm, we just tried it. It will fail properly later in AutoForm which should be easier to understand.
                }
            }

            if (visibleResourceBinding.HasBinding || visibleResourceBinding.ValueOrDefault is not true)
            {
                field.SetValue(DotvvmControl.IncludeInPageProperty, visibleResourceBinding);
            }

        }

        [DotvvmControlCapability]
        public sealed record FieldSelectorProps
        {
            /// <summary> Only the specified properties will be included in this form. Using ViewName, GroupName or ExcludedProperties at the same time as IncludedProperties does not make sense. The properties will be listed in the exact order defined in this property. </summary>
            public string[]? IncludeProperties { get; init; }

            /// <summary> The specified properties will not be included in this form. </summary>
            public string[] ExcludeProperties { get; init; } = new string[0];

            /// <summary> Adds or overrides the property binding. </summary>
            [PropertyGroup("Property-")]
            public IReadOnlyDictionary<string, IValueBinding> Property { get; init; } = new Dictionary<string, IValueBinding>();

            /// <summary> Gets or sets the view name (e.g. Insert, Edit, ReadOnly). Some fields may have different metadata for each view. </summary>
            public string? ViewName { get; init; }

            /// <summary> Gets or sets the group of fields that should be rendered. If not set, fields from all groups will be rendered. </summary>
            public string? GroupName { get; init; }
        }

        [DotvvmControlCapability]
        public sealed record FieldProps
        {
            /// <summary> Calls the command when the user makes changes to the specified field. For example `Changed-CountryId="{staticCommand: _root.States.Items = statesDataProvider.GetSelectorItems(_root.Address).Result}"` will reload the list of states whenever CountryId is changed. </summary>
            [PropertyGroup("Changed-")]
            public IReadOnlyDictionary<string, ICommandBinding> Changed { get; init; } = new Dictionary<string, ICommandBinding>();

            /// <summary> Controls if the specified property is editable. </summary>
            [PropertyGroup("Enabled-")]
            public IReadOnlyDictionary<string, ValueOrBinding<bool>> Enabled { get; init; } = new Dictionary<string, ValueOrBinding<bool>>();

            /// <summary> Controls if the specified field is visible </summary>
            [PropertyGroup("Visible-")]
            public IReadOnlyDictionary<string, ValueOrBinding<bool>> Visible { get; init; } = new Dictionary<string, ValueOrBinding<bool>>();


            /// <summary> Overrides which text is used as a field label. </summary>
            [PropertyGroup("Label-")]
            public IReadOnlyDictionary<string, ValueOrBinding<string>> Label { get; init; } = new Dictionary<string, ValueOrBinding<string>>();

            /// <summary> Overrides how the entire form field (editor, label, ...) looks like. </summary>
            [PropertyGroup("FieldTemplate-")]
            [MarkupOptions(MappingMode = MappingMode.InnerElement)]
            public IReadOnlyDictionary<string, ITemplate> FieldTemplate { get; init; } = new Dictionary<string, ITemplate>();


            /// <summary> Overrides which component is used as an editor. </summary>
            [PropertyGroup("EditorTemplate-")]
            [MarkupOptions(MappingMode = MappingMode.InnerElement)]
            public IReadOnlyDictionary<string, ITemplate> EditorTemplate { get; init; } = new Dictionary<string, ITemplate>();

            public FieldSelectorProps FieldSelector { get; init; } = new FieldSelectorProps();

        }
    }
}
