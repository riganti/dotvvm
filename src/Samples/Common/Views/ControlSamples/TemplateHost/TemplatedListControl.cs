using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.ControlSamples.TemplateHost
{
    public class TemplatedListControl : DotvvmMarkupControl
    {

        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public IEnumerable DataSource
        {
            get { return (IEnumerable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }
        public static readonly DotvvmProperty DataSourceProperty
            = DotvvmProperty.Register<IEnumerable, TemplatedListControl>(c => c.DataSource, null);

        [ControlPropertyBindingDataContextChange(nameof(DataSource), order: 0)]
        [CollectionElementDataContextChange(order: 1)]
        [MarkupOptions(AllowBinding = false, Required = true, MappingMode = MappingMode.InnerElement)]
        public ITemplate ItemTemplate
        {
            get { return (ITemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public static readonly DotvvmProperty ItemTemplateProperty
            = DotvvmProperty.Register<ITemplate, TemplatedListControl>(c => c.ItemTemplate, null);

        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public ICommandBinding OnCreateItem
        {
            get { return (ICommandBinding)GetValue(OnCreateItemProperty); }
            set { SetValue(OnCreateItemProperty, value); }
        }
        public static readonly DotvvmProperty OnCreateItemProperty
            = DotvvmProperty.Register<ICommandBinding, TemplatedListControl>(c => c.OnCreateItem, null);


        public void AddItem()
        {
            var item = OnCreateItem.BindingDelegate(this.GetDataContexts().ToArray(), this);
            ((dynamic)DataSource).Add(((dynamic)item)());
        }

        public void RemoveItem(object item)
        {
            ((dynamic)DataSource).Remove((dynamic)item);
        }
        
    }

}

