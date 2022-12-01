using System;
using System.Linq;
using DotVVM.AutoUI.PropertyHandlers;
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
    public sealed class AutoEditor : CompositeControl
    {
        
        private readonly IServiceProvider services;

        public AutoEditor(IServiceProvider services)
        {
            this.services = services;
        }

        public DotvvmControl GetContents(Props props)
        {
            var autoUiContext = new AutoUIContext(this.GetDataContextType().NotNull(), services);
            return Build(props, autoUiContext);
        }

        public static DotvvmControl Build(Props props, AutoUIContext autoUiContext)
        {
            if (props.OverrideTemplate is { })
            {
                return new TemplateHost(props.OverrideTemplate);
            }

            if (props.Property is null)
                throw new DotvvmControlException($"{nameof(props.Property)} is not set.");

            var prop = props.Property.GetProperty<ReferencedViewModelPropertiesBindingProperty>();

            if (prop.MainProperty is null)
                throw new NotSupportedException($"The binding {props.Property} must be bound to a single property. Alternatively, you can write a custom server-side style rule for your expression.");

            var propertyMetadata = autoUiContext.PropertyDisplayMetadataProvider.GetPropertyMetadata(prop.MainProperty);

            var editorProvider =
                autoUiContext.AutoUiConfiguration.FormEditorProviders.FindBestProvider(propertyMetadata, autoUiContext);

            if (editorProvider is null)
                throw new DotvvmControlException($"Editor provider for property {prop.MainProperty} could not be found.");

            return editorProvider.CreateControl(propertyMetadata, props, autoUiContext);
        }

        [DotvvmControlCapability]
        public sealed record Props
        {
            public IValueBinding? Property { get; init; }
            public ICommandBinding? Changed { get; init; }
            public ValueOrBinding<bool> Enabled { get; init; } = new(true);
            public HtmlCapability Html { get; init; } = new();
            public ITemplate? OverrideTemplate { get; init; }
        }
    }
}
