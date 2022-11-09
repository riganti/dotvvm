using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary> Control which initializes a client-side component. </summary>
    public class JsComponent : DotvvmControl
    {
        /// <summary> If set to true, only globally registered JsComponents will be considered for rendering client-side. </summary>
        public bool Global
        {
            get { return (bool)GetValue(GlobalProperty)!; }
            set { SetValue(GlobalProperty, value); }
        }
        public static readonly DotvvmProperty GlobalProperty =
            DotvvmProperty.Register<bool, JsComponent>(nameof(Global));

        /// <summary> Name by which the client-side component was registered. The name is case sensitive. </summary>
        [MarkupOptions(Required = true)]
        public string Name
        {
            get { return (string)GetValue(NameProperty)!; }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DotvvmProperty NameProperty =
            DotvvmProperty.Register<string, JsComponent>(nameof(Name));

        /// <summary> The JsComponent must have a wrapper HTML tag, this property configures which tag is used. By default, `div` is used. </summary>
        public string WrapperTagName
        {
            get { return (string)GetValue(WrapperTagNameProperty)!; }
            set { SetValue(WrapperTagNameProperty, value); }
        }
        public static readonly DotvvmProperty WrapperTagNameProperty =
            DotvvmProperty.Register<string, JsComponent>(nameof(WrapperTagName), "div");

        /// <summary>
        /// The properties passed into the JsComponent. The properties may contain any object from the viewModel, command or a staticCommand binding.
        /// </summary>
        [PropertyGroup(new[] { "", "prop:" })]
        public VirtualPropertyGroupDictionary<object?> Props => new(this, PropsGroupDescriptor);
        public static DotvvmPropertyGroup PropsGroupDescriptor =
            DotvvmPropertyGroup.Register<object, JsComponent>(new [] { "", "prop:" }, nameof(Props));

        /// <summary>
        /// Templates to pass into the JsComponent. The templates will be rendered as knockout templates and the client-side component will get their ids. In React, the KnockoutTemplateReactComponent can be used to add it to the virtual DOM.
        /// </summary>
        [PropertyGroup(new[] { "", "template-" })]
        public VirtualPropertyGroupDictionary<ITemplate> Templates => new(this, TemplatesGroupDescriptor);
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public static DotvvmPropertyGroup TemplatesGroupDescriptor =
            DotvvmPropertyGroup.Register<ITemplate, JsComponent>(new [] { "", "template-" }, nameof(Templates));

        public HtmlCapability HtmlCapability
        {
            get { return (HtmlCapability)GetValue(HtmlCapabilityProperty)!; }
            set { SetValue(HtmlCapabilityProperty, value); }
        }

        /// <summary> Allows modifying the attributes of the wrapper html tag (`div` by default) </summary>
        public static DotvvmCapabilityProperty HtmlCapabilityProperty =
            DotvvmCapabilityProperty.RegisterCapability<HtmlCapability, JsComponent>(
                globalPrefix: "html:",
                name: "HtmlCapability"
            );

        public JsComponent()
        {
        }

        public JsComponent(string name)
        {
            this.Name = name;
        }

        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            Children.Clear();
            foreach (var (name, template) in Templates)
            {
                var placeholder = new PlaceHolder();
                this.Children.Add(placeholder);
                placeholder.ID = name;
                InitializeTemplate(template, placeholder, context);
            }

            base.OnLoad(context);
        }

        protected virtual void InitializeTemplate(ITemplate template, PlaceHolder placeholder, IDotvvmRequestContext context)
        {
            template.BuildContent(context, placeholder);
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var templates = new KnockoutBindingGroup();
            foreach (var placeholder in this.Children)
                templates.AddValue(
                    placeholder.ID.NotNull(),
                    context.ResourceManager.AddTemplateResource(context, placeholder)
                );

            var commands = new KnockoutBindingGroup();
            var props = new KnockoutBindingGroup();
            foreach (var (name, value) in Props.RawValues)
            {
                if (value is ICommandBinding command)
                {
                    commands.Add(name, KnockoutHelper.GenerateClientPostbackLambda(name, command, this));
                }
                else if (value is IValueBinding valueBinding)
                {
                    props.Add(name, valueBinding.GetKnockoutBindingExpression(this));
                }
                else
                {
                    props.AddValue(name, EvalPropertyValue(NameProperty, value));
                }
            }

            var binding = new KnockoutBindingGroup {
                { "name", this, NameProperty },
                { "commands", commands },
                { "props", props },
                { "templates", templates },
            };
            if (GetValue(Internal.ReferencedViewModuleInfoProperty) is ViewModuleReferenceInfo viewModule)
                binding.Add("view", ViewModuleHelpers.GetViewIdJsExpression(viewModule, this));

            writer.AddKnockoutDataBind("dotvvm-js-component", binding);
            var htmlElement = new HtmlGenericControl(WrapperTagName, this.HtmlCapability);
            htmlElement.Render(writer, context);
        }

        [ApplyControlStyle]
        public static void AddReferencedViewModuleInfoProperty(ResolvedControl control)
        {
            if (control.TreeRoot.TryGetProperty(Internal.ReferencedViewModuleInfoProperty, out var x))
            {
                var value = ((ResolvedPropertyValue)x).Value;
                control.SetProperty(new ResolvedPropertyValue(Internal.ReferencedViewModuleInfoProperty, value));
            }
            if (control.ConstructorParameters != null)
            {
                control.SetProperty(new ResolvedPropertyValue(NameProperty, control.ConstructorParameters.Single()));
                control.ConstructorParameters = null;
            }
        }
    }
}
