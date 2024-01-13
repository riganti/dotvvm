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
        public ITemplate After
        {
            get => (ITemplate)GetValue(AfterProperty)!;
            set => SetValue(AfterProperty, value);
        }
        public static readonly DotvvmProperty AfterProperty =
            DotvvmProperty.Register<ITemplate, AddTemplateDecorator>(nameof(After));

        /// <summary> Template is rendered before the decorated control. </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate Before
        {
            get => (ITemplate)GetValue(BeforeProperty)!;
            set => SetValue(BeforeProperty, value);
        }
        public static readonly DotvvmProperty BeforeProperty =
            DotvvmProperty.Register<ITemplate, AddTemplateDecorator>(nameof(Before));

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            var after = this.After;
            var before = this.Before;

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
