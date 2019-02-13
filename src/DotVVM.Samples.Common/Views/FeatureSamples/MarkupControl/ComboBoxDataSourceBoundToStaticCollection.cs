using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.Views.FeatureSamples.MarkupControl
{
    public class ComboBoxDataSourceBoundToStaticCollection : DotvvmMarkupControl
    {
        public List<Detail> DataSource
        {
            get { return (List<Detail>)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }
        public static readonly DotvvmProperty DataSourceProperty
            = DotvvmProperty.Register<List<Detail>, ComboBoxDataSourceBoundToStaticCollection>(c => c.DataSource, null);

        public int SelectedValue
        {
            get { return (int)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }
        public static readonly DotvvmProperty SelectedValueProperty
            = DotvvmProperty.Register<int, ComboBoxDataSourceBoundToStaticCollection>(c => c.SelectedValue, default(int));

        public Detail Item
        {
            get { return (Detail)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }
        public static readonly DotvvmProperty ItemProperty
            = DotvvmProperty.Register<Detail, ComboBoxDataSourceBoundToStaticCollection>(c => c.Item);

        protected override void OnInit(IDotvvmRequestContext context)
        {
            base.OnInit(context);

            DataSource = Enumerable.Range(0, 3)
                .Select(x => new Detail {
                    Id = x,
                    InnerText = new Detail.InnerItem { Text = string.Format("Number {0}", x) }
                }).ToList();

            Item = new Detail { Id = 0, InnerText = new Detail.InnerItem { Text = "Default item" } };
        }

        public class Detail
        {
            public int Id { get; set; }

            public InnerItem InnerText { get; set; }

            public class InnerItem
            {
                public string Text { get; set; }
            }
        }
    }
}
