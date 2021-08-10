using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class TextRepeater : DotvvmControl
    {
        private readonly BindingCompilationService bindingService;

        public TextRepeater(BindingCompilationService bindingService)
        {
            this.bindingService = bindingService;
        }

        [MarkupOptions(AllowHardCodedValue = false)]
        [BindingCompilationRequirements(new[] { typeof(DataSourceAccessBinding) }, new[] { typeof(DataSourceLengthBinding) })]
        public virtual object DataSource
        {
            get => (object)GetValue(DataSourceProperty);
            set => SetValue(DataSourceProperty, value);
        }

        public static readonly DotvvmProperty DataSourceProperty =
            DotvvmProperty.Register<object, TextRepeater>(t => t.DataSource);

        [CollectionElementDataContextChange(1)]
        [ControlPropertyBindingDataContextChange(nameof(DataSource))]
        public IValueBinding<string> ItemTextBinding
        {
            get => (IValueBinding<string>)GetValue(ItemTextBindingProperty);
            set => SetValue(ItemTextBindingProperty, value);
        }

        public static readonly DotvvmProperty ItemTextBindingProperty =
            DotvvmProperty.Register<IValueBinding<string>, TextRepeater>(t => t.ItemTextBinding);

        protected override void OnLoad(IDotvvmRequestContext context)
        {
            base.OnLoad(context);
            SetChildren();
        }

        protected override void OnPreRender(IDotvvmRequestContext context)
        {
            SetChildren();
            base.OnPreRender(context);
        }

        private void SetChildren()
        {
            Children.Clear();

            var repeater = new Repeater();
            {
                repeater.SetBinding(c => c.DataSource, GetValueBinding(DataSourceProperty));
                repeater.ItemTemplate = new DelegateTemplate(_ => {
                    return new Literal()
                        .SetBinding(c => c.Text, ItemTextBinding);
                });
            };

            Children.Add(repeater);
        }
    }
}
