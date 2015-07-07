using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    public class GridView : ItemsControl
    {

        public GridView() : base("table")
        {
            Columns = new List<GridViewColumn>();
            RowDecorators = new List<Decorator>();
        }


        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public List<GridViewColumn> Columns
        {
            get { return (List<GridViewColumn>)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }
        public static readonly DotvvmProperty ColumnsProperty =
            DotvvmProperty.Register<List<GridViewColumn>, GridView>(c => c.Columns);


        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public List<Decorator> RowDecorators
        {
            get { return (List<Decorator>)GetValue(RowDecoratorsProperty); }
            set { SetValue(RowDecoratorsProperty, value); }
        }
        public static readonly DotvvmProperty RowDecoratorsProperty =
            DotvvmProperty.Register<List<Decorator>, GridView>(c => c.RowDecorators);


        [MarkupOptions(AllowHardCodedValue = false)]
        public Action<string> SortChanged
        {
            get { return (Action<string>)GetValue(SortChangedProperty); }
            set { SetValue(SortChangedProperty, value); }
        }
        public static readonly DotvvmProperty SortChangedProperty =
            DotvvmProperty.Register<Action<string>, GridView>(c => c.SortChanged, null);


        protected internal override void OnLoad(DotvvmRequestContext context)
        {
            DataBind(context);
            base.OnLoad(context);
        }

        protected internal override void OnPreRender(DotvvmRequestContext context)
        {
            DataBind(context);     // TODO: support for observable collection
            base.OnPreRender(context);
        }


        private void DataBind(DotvvmRequestContext context)
        {
            Children.Clear();

            var dataSourceBinding = GetDataSourceBinding();
            var dataSourcePath = dataSourceBinding.GetViewModelPathExpression(this, DataSourceProperty);
            var dataSource = DataSource;

            string sortCommandPath = "";
            if (dataSource is IGridViewDataSet)
            {
                sortCommandPath = dataSourcePath + ".SetSortExpression";
            }
            else
            {
                var sortCommandBinding = GetCommandBinding(SortChangedProperty);
                if (sortCommandBinding != null)
                {
                    sortCommandPath = sortCommandBinding.Expression;
                }
            }

            var index = 0;
            if (dataSource != null)
            {
                // create header row
                CreateHeaderRow(context, sortCommandPath);

                foreach (var item in GetIEnumerableFromDataSource(dataSource))
                {
                    // create row
                    var placeholder = new DataItemContainer { DataItemIndex = index };
                    placeholder.SetBinding(DataContextProperty, new ValueBindingExpression(dataSourcePath + "[" + index + "]"));
                    Children.Add(placeholder);

                    CreateRow(context, placeholder);

                    index++;
                }
            }
        }

        private void CreateHeaderRow(DotvvmRequestContext context, string sortCommandPath)
        {
            var headerRow = new HtmlGenericControl("tr");
            Children.Add(headerRow);
            foreach (var column in Columns)
            {
                var cell = new HtmlGenericControl("th");
                SetCellAttributes(column, cell, true);
                headerRow.Children.Add(cell);

                column.CreateHeaderControls(context, this, sortCommandPath, cell);
            }
        }

        private static void SetCellAttributes(GridViewColumn column, HtmlGenericControl cell, bool isHeaderCell)
        {
            if (!string.IsNullOrEmpty(column.Width))
            {
                cell.Attributes["style"] = "width: " + column.Width;
            }

            if (!isHeaderCell)
            {
                var cssClassBinding = column.GetValueBinding(GridViewColumn.CssClassProperty);
                if (cssClassBinding != null)
                {
                    cell.Attributes["class"] = cssClassBinding.Clone();
                }
                else if (!string.IsNullOrWhiteSpace(column.CssClass))
                {
                    cell.Attributes["class"] = column.CssClass;
                }
            }
        }

        private void CreateRow(DotvvmRequestContext context, DataItemContainer placeholder)
        {
            var row = new HtmlGenericControl("tr");

            DotvvmControl container = row;
            foreach (var decorator in RowDecorators)
            {
                var decoratorInstance = decorator.Clone();
                decoratorInstance.Children.Add(container);
                container = decoratorInstance;
            }
            placeholder.Children.Add(container);

            // create cells
            foreach (var column in Columns)
            {
                var cell = new HtmlGenericControl("td");
                SetCellAttributes(column, cell, false);
                row.Children.Add(cell);
                column.CreateControls(context, cell);
            }
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            if (Children.Count == 0) return;

            // render the header
            Children[0].Render(writer, context);

            // render body
            var dataSourceBinding = GetDataSourceBinding();
            if (!RenderOnServer)
            {
                var expression = dataSourceBinding.TranslateToClientScript(this, DataSourceProperty);
                writer.AddKnockoutForeachDataBind(expression);
            }
            writer.RenderBeginTag("tbody");

            // render contents
            if (RenderOnServer)
            {
                // render on server
                var index = 0;
                foreach (var child in Children.Skip(1))
                {
                    context.PathFragments.Push(dataSourceBinding.GetViewModelPathExpression(this, DataSourceProperty) + "[" + index + "]");
                    Children[index].Render(writer, context);
                    context.PathFragments.Pop();
                    index++;
                }
            }
            else
            {
                // render on client
                var placeholder = new DataItemContainer { DataContext = null };
                Children.Add(placeholder);

                CreateRow(context.RequestContext, placeholder);

                context.PathFragments.Push(dataSourceBinding.GetViewModelPathExpression(this, DataSourceProperty) + "[$index]");
                placeholder.Render(writer, context);
                context.PathFragments.Pop();
            }

            writer.RenderEndTag();
        }
    }
}
