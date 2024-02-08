using DotVVM.Framework.Hosting;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    /// <summary> Adds a template before or after the decorated control. </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class AddTemplateDecorator: Decorator
    {
        /// <summary> Template is rendered after the decorated control. </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate AfterTemplate
        {
            get => (ITemplate)GetValue(AfterTemplateProperty)!;
            set => SetValue(AfterTemplateProperty, value);
        }
        public static readonly DotvvmProperty AfterTemplateProperty =
            DotvvmProperty.Register<ITemplate, AddTemplateDecorator>(nameof(AfterTemplate));

        /// <summary> Template is rendered before the decorated control. </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate BeforeTemplate
        {
            get => (ITemplate)GetValue(BeforeTemplateProperty)!;
            set => SetValue(BeforeTemplateProperty, value);
        }
        public static readonly DotvvmProperty BeforeTemplateProperty =
            DotvvmProperty.Register<ITemplate, AddTemplateDecorator>(nameof(BeforeTemplate));

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            var after = this.AfterTemplate;
            var before = this.BeforeTemplate;

            if (after is {})
            {
                Children.Add(new TemplateHost(after));
            }
            if (before is {})
            {
                Children.Insert(0, new TemplateHost(before));
            }
        }
    }
}
