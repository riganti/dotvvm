using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{

	public interface IHierarchyItem
	{
		IEnumerable<IHierarchyItem> Children { get; }
	}
	public class TreeView : DotvvmControl
	{
		public IEnumerable<IHierarchyItem> DataSource
		{
			get { return (IEnumerable<IHierarchyItem>)GetValue(DataSourceProperty); }
			set { SetValue(DataSourceProperty, value); }
		}
		public static readonly DotvvmProperty DataSourceProperty
			= DotvvmProperty.Register<IEnumerable<IHierarchyItem>, TreeView>(c => c.DataSource, null);

		// change DataContext in ItemTemplate to DataSource element type
		[ControlPropertyBindingDataContextChange(nameof(DataSource), order: 0)]
		[CollectionElementDataContextChange(order: 1)]
		[MarkupOptions(MappingMode = MappingMode.InnerElement)]
		public ITemplate ItemTemplate { get; set; }

		public void DataBind(IDotvvmRequestContext context)
		{
			Children.Clear();
			foreach (var item in DataSource)
			{
				DataBindItem(this, item, context);
			}
		}

		public void DataBindItem(DotvvmControl parent, IHierarchyItem item, IDotvvmRequestContext context)
		{
			// render item template
			var templatePlaceholder = new PlaceHolder();
			templatePlaceholder.DataContext = item;
			parent.Children.Add(templatePlaceholder);

			ItemTemplate.BuildContent(context, templatePlaceholder);
			if (item.Children.Any())
			{
				// wrap children in div
				var childContainer = new HtmlGenericControl("div");
				childContainer.Attributes["class"] = "child-container";
				foreach (var child in item.Children)
				{
					DataBindItem(childContainer, child, context);
				}
			}
		}

		protected internal override void OnLoad(IDotvvmRequestContext context)
		{
			DataBind(context);
			base.OnInit(context);
		}

		protected internal override void OnPreRender(IDotvvmRequestContext context)
		{
			DataBind(context);
			base.OnLoad(context);
		}
	}
}
