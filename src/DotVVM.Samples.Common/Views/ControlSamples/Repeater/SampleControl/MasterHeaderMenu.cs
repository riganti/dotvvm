using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.Views.ControlSamples.Repeater.SampleControl
{
    public class MasterHeaderMenu : HtmlGenericControl
    {
        public MasterHeaderMenu() : base("div")
        {
        }

        [MarkupOptions(AllowHardCodedValue = false)]
        [CollectionElementDataContextChange(1)]
        [ControlPropertyBindingDataContextChange(nameof(DataSource))]
        public ICommandBinding MenuClick
        {
            get => (ICommandBinding)GetValue(MenuClickProperty);
            set => SetValue(MenuClickProperty, value);
        }

        public static readonly DotvvmProperty MenuClickProperty =
            DotvvmProperty.Register<ICommandBinding, MasterHeaderMenu>(t => t.MenuClick);

        [MarkupOptions(AllowHardCodedValue = false)]
        //[BindingCompilationRequirements(new[] { typeof(DataSourceAccessBinding) }, new[] { typeof(DataSourceLengthBinding) })]
        public IEnumerable<object> DataSource
        {
            get => (IEnumerable<object>)GetValue(DataSourceProperty);
            set => SetValue(DataSourceProperty, value);
        }

        public static readonly DotvvmProperty DataSourceProperty
            = DotvvmProperty.Register<IEnumerable<object>, MasterHeaderMenu>(t => t.DataSource);

        [CollectionElementDataContextChange(1)]
        [ControlPropertyBindingDataContextChange(nameof(DataSource))]
        //[BindingCompilationRequirements(required: new[] { typeof(SelectorItemBindingProperty) })]
        public IValueBinding ItemTextBinding
        {
            get => (IValueBinding)GetValue(ItemTextBindingProperty);
            set => SetValue(ItemTextBindingProperty, value);
        }
        public static readonly DotvvmProperty ItemTextBindingProperty =
            DotvvmProperty.Register<IValueBinding, MasterHeaderMenu>(t => t.ItemTextBinding);

        [CollectionElementDataContextChange(1)]
        [ControlPropertyBindingDataContextChange(nameof(DataSource))]
        //[BindingCompilationRequirements(required: new[] { typeof(SelectorItemBindingProperty) })]
        public IValueBinding ItemIdBinding
        {
            get => (IValueBinding)GetValue(ItemIdBindingProperty);
            set => SetValue(ItemIdBindingProperty, value);
        }

        public static readonly DotvvmProperty ItemIdBindingProperty =
            DotvvmProperty.Register<IValueBinding, MasterHeaderMenu>(t => t.ItemIdBinding);

        [MarkupOptions(AllowHardCodedValue = false)]
        public string SelectedItemId
        {
            get => (string)GetValue(SelectedItemIdProperty);
            set => SetValue(SelectedItemIdProperty, value);
        }
        public static readonly DotvvmProperty SelectedItemIdProperty =
            DotvvmProperty.Register<string, MasterHeaderMenu>(c => c.SelectedItemId, null);

        protected override void OnInit(IDotvvmRequestContext context)
        {
            var repeater = new Framework.Controls.Repeater();
            Children.Add(repeater);
            repeater.WrapperTagName = "ul";
            repeater.SetBinding(c => c.DataSource, GetValueBinding(DataSourceProperty));
            repeater.ItemTemplate = new DelegateTemplate(BuildItemTemplate);

            base.OnInit(context);
        }

        protected virtual void BuildItemTemplate(IServiceProvider services, DotvvmControl container)
        {
            var item = new HtmlGenericControl("li");
            container.Children.Add(item);

            var rb = new RadioButton();
            item.Children.Add(rb);

            rb.SetValue(CheckableControlBase.TextProperty, GetValueBinding(ItemTextBindingProperty));
            rb.SetValue(CheckableControlBase.CheckedValueProperty, GetValueBinding(ItemIdBindingProperty));
            rb.SetValue(RadioButton.CheckedItemProperty, GetValueBinding(SelectedItemIdProperty));

            //!!!!!!!!!!!!!!!!!!!!!
            IBinding binding = GetCommandBinding(MenuClickProperty);
            var dataContextStack = binding.GetProperty<DataContextStack>();
            rb.SetBinding(b => b.Changed, binding);

            IBinding binding2 = GetValueBinding(ItemTextBindingProperty);
            var dataContextStack2 = binding2.GetProperty<DataContextStack>();
            //!!!!!!!!!!!!!!!!!!!!!
        }
    }
}
