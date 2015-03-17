using Redwood.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
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
        public static readonly RedwoodProperty ColumnsProperty =
            RedwoodProperty.Register<List<GridViewColumn>, GridView>(c => c.Columns);


        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public List<Decorator> RowDecorators
        {
            get { return (List<Decorator>)GetValue(RowDecoratorsProperty); }
            set { SetValue(RowDecoratorsProperty, value); }
        }
        public static readonly RedwoodProperty RowDecoratorsProperty =
            RedwoodProperty.Register<List<Decorator>, GridView>(c => c.RowDecorators);



        protected internal override void OnLoad(RedwoodRequestContext context)
        {
            DataBind();
            base.OnLoad(context);
        }

        protected internal override void OnPreRender(RedwoodRequestContext context)
        {
            DataBind();     // TODO: support for observable collection
            base.OnPreRender(context);
        }


        private void DataBind()
        {
            Children.Clear();

            var dataSourceBinding = GetDataSourceBinding();
            var dataSourcePath = dataSourceBinding.GetViewModelPathExpression(this, DataSourceProperty);

            var index = 0;
            if (DataSource != null)
            {
                // create header row
                var headerRow = new HtmlGenericControl("th");
                Children.Add(headerRow);
                foreach (var column in Columns)
                {
                    var cell = new HtmlGenericControl("th");
                    SetCellAttributes(column, cell);
                    headerRow.Children.Add(cell);

                    column.CreateHeaderControls(cell);
                }

                foreach (var item in DataSource)
                {
                    // create row
                    var placeholder = new DataItemContainer { DataItemIndex = index };
                    placeholder.SetBinding(DataContextProperty, new ValueBindingExpression(dataSourcePath + "[" + index + "]"));
                    Children.Add(placeholder);

                    CreateRow(placeholder);

                    index++;
                }
            }
        }

        private static void SetCellAttributes(GridViewColumn column, HtmlGenericControl cell)
        {
            if (!string.IsNullOrEmpty(column.Width))
            {
                cell.Attributes["style"] = "width: " + column.Width;
            }
            if (!string.IsNullOrEmpty(column.CssClass))
            {
                cell.Attributes["class"] = column.CssClass;
            }
        }

        private void CreateRow(DataItemContainer placeholder)
        {
            var row = new HtmlGenericControl("tr");

            RedwoodControl container = row;
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
                SetCellAttributes(column, cell);
                row.Children.Add(cell);
                column.CreateControls(cell);
            }
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            if (!RenderOnServer)
            {
                writer.AddKnockoutDataBind("foreach", this, DataSourceProperty, () => { });
            }

            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            if (Children.Count == 0) return;

            // render the header
            Children[0].Render(writer, context);

            // render contents
            var dataSourceBinding = GetDataSourceBinding();
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

                CreateRow(placeholder);

                context.PathFragments.Push(dataSourceBinding.GetViewModelPathExpression(this, DataSourceProperty) + "[$index]");
                placeholder.Render(writer, context);
                context.PathFragments.Pop();
            }
        }

    }
}
