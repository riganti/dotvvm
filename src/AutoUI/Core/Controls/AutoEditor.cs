using System;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.AutoUI.Controls
{
    /// <summary>
    /// Creates the editor for the specified property using the metadata information.
    /// </summary>
    [ControlMarkupOptions(PrimaryName = "Editor", AllowContent = false, Precompile = ControlPrecompilationMode.InServerSideStyles)]
    public sealed class AutoEditor : CompositeControl, IObjectWithCapability<HtmlCapability>, IControlWithHtmlAttributes
    {
        /// <summary>
        /// Gets or sets the property that should be edited.
        /// </summary>
        [BindingCompilationRequirements(required: new[] { typeof(ReferencedViewModelPropertiesBindingProperty) })]
        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public IValueBinding? Property
        {
            get { return (IValueBinding?)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }

        public VirtualPropertyGroupDictionary<object?> Attributes =>
            new(this, AttributesGroupDescriptor);
        [MarkupOptions(MappingMode = MappingMode.Attribute, AllowBinding = true, AllowHardCodedValue = true, AllowValueMerging = true, AttributeValueMerger = typeof(HtmlAttributeValueMerger), AllowAttributeWithoutValue = true)]
        public static DotvvmPropertyGroup AttributesGroupDescriptor =
            DotvvmPropertyGroup.Register<object, AutoEditor>(new[] { "", "html:" }, nameof(Attributes));

        public static readonly DotvvmProperty PropertyProperty
            = DotvvmProperty.Register<IValueBinding, AutoEditor>(c => c.Property, null);


        public ITemplate? OverrideTemplate
        {
            get { return (ITemplate?)GetValue(OverrideTemplateProperty); }
            set { SetValue(OverrideTemplateProperty, value); }
        }
        public static readonly DotvvmProperty OverrideTemplateProperty =
            DotvvmProperty.Register<ITemplate, AutoEditor>(c => c.OverrideTemplate, null);

        public string[] Tags
        {
            get { return (string[]?)GetValue(TagsProperty) ?? Array.Empty<string>(); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly DotvvmProperty TagsProperty =
            DotvvmProperty.Register<string[], AutoEditor>("Tags", Array.Empty<string>());

        private readonly IServiceProvider services;

        public AutoEditor(IServiceProvider services)
        {
            this.services = services;
        }

        public DotvvmControl GetContents(Props props)
        {
            if (props.OverrideTemplate is { })
            {
                return new TemplateHost(props.OverrideTemplate);
            }

            var context = new AutoUIContext(this.GetDataContextType().NotNull(), services) {
                ViewName = null,
                GroupName = null
            };

            if (Property is null)
                throw new DotvvmControlException(this, $"{nameof(Property)} is not set.");

            var prop = Property.GetProperty<ReferencedViewModelPropertiesBindingProperty>();

            if (prop.MainProperty is null)
                throw new NotSupportedException($"The binding {Property} must be bound to a single property. Alternatively, you can write a custom server-side style rule for your expression.");

            var propertyMetadata = context.PropertyDisplayMetadataProvider.GetPropertyMetadata(prop.MainProperty);

            var editorProvider =
                context.AutoUiConfiguration.FormEditorProviders
                    .FirstOrDefault(e => e.CanHandleProperty(propertyMetadata, context));

            if (editorProvider is null)
                throw new DotvvmControlException(this, $"Editor provider for property {prop.MainProperty} could not be found.");

            var control = editorProvider.CreateControl(propertyMetadata, props, context);
            if (props.Tags.Length > 0)
                control.SetValue(TagsProperty, props.Tags);
            return control;
        }

        [DotvvmControlCapability]
        public sealed record Props
        {
            public IValueBinding? Property { get; init; }
            public ICommandBinding? Changed { get; init; }
            public ValueOrBinding<bool> Enabled { get; init; } = new(true);
            public HtmlCapability Html { get; init; } = new();
            public ITemplate? OverrideTemplate { get; init; }
            /// <summary> Internal tags for styles to match on. </summary>
            public string[] Tags { get; init; } = new string[0];
        }
    }
}
