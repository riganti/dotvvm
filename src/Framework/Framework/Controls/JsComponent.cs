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
    public class JsComponent : DotvvmControl
    {
        public bool Global
        {
            get { return (bool)GetValue(GlobalProperty)!; }
            set { SetValue(GlobalProperty, value); }
        }
        public static readonly DotvvmProperty GlobalProperty =
            DotvvmProperty.Register<bool, JsComponent>(nameof(Global));

        // [MarkupOptions(Required = true)]
        public string Name
        {
            get { return (string)GetValue(NameProperty)!; }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DotvvmProperty NameProperty =
            DotvvmProperty.Register<string, JsComponent>(nameof(Name));

        public string WrapperTagName
        {
            get { return (string)GetValue(WrapperTagNameProperty)!; }
            set { SetValue(WrapperTagNameProperty, value); }
        }
        public static readonly DotvvmProperty WrapperTagNameProperty =
            DotvvmProperty.Register<string, JsComponent>(nameof(WrapperTagName), "div");

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        [PropertyGroup(new[] { "", "prop:" })]
        public VirtualPropertyGroupDictionary<object?> Props => new(this, PropsGroupDescriptor);
        public static DotvvmPropertyGroup PropsGroupDescriptor =
            DotvvmPropertyGroup.Register<object, JsComponent>(new [] { "", "prop:" }, nameof(Props));

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        [PropertyGroup(new[] { "", "template-" })]
        public VirtualPropertyGroupDictionary<ITemplate> Templates => new(this, TemplatesGroupDescriptor);
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public static DotvvmPropertyGroup TemplatesGroupDescriptor =
            DotvvmPropertyGroup.Register<ITemplate, JsComponent>(new [] { "", "template-" }, nameof(Templates));

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
                template.BuildContent(context, placeholder);
            }

            base.OnLoad(context);
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

            writer.RenderBeginTag(WrapperTagName);
            writer.RenderEndTag();
        }

        [ApplyControlStyle]
        public static void AddReferencedViewModuleInfoProperty(ResolvedControl control)
        {
            if (control.TreeRoot.TryGetProperty(Internal.ReferencedViewModuleInfoProperty, out var x))
            {
                var value = ((ResolvedPropertyValue)x).Value;
                control.SetProperty(new ResolvedPropertyValue(Internal.ReferencedViewModuleInfoProperty, value));
            }
        }
    }
}
