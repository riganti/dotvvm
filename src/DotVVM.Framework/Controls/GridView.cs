using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using System.Collections;
using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A multi-purpose grid control with advanced binding and templating options and sorting support.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = nameof(Columns))]
    public class GridView : ItemsControl
    {
        private EmptyData emptyDataContainer;
        private int numberOfRows;
        private HtmlGenericControl head;

        public GridView() : base("table")
        {
            SetValue(Internal.IsNamingContainerProperty, true);

            Columns = new List<GridViewColumn>();
            RowDecorators = new List<Decorator>();
        }


        /// <summary>
        /// Gets or sets the place where the filters will be created.
        /// </summary>
        public GridViewFilterPlacement FilterPlacement
        {
            get { return (GridViewFilterPlacement)GetValue(FilterPlacementProperty); }
            set { SetValue(FilterPlacementProperty, value); }
        }
        public static readonly DotvvmProperty FilterPlacementProperty
            = DotvvmProperty.Register<GridViewFilterPlacement, GridView>(c => c.FilterPlacement, GridViewFilterPlacement.HeaderRow);


        /// <summary>
        /// Gets or sets the template which will be displayed when the DataSource is empty.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate EmptyDataTemplate
        {
            get { return (ITemplate)GetValue(EmptyDataTemplateProperty); }
            set { SetValue(EmptyDataTemplateProperty, value); }
        }
        public static readonly DotvvmProperty EmptyDataTemplateProperty =
            DotvvmProperty.Register<ITemplate, GridView>(t => t.EmptyDataTemplate, null);


        /// <summary>
        /// Gets or sets a collection of columns that will be placed inside the grid.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        [ControlPropertyBindingDataContextChange("DataSource")]
        [CollectionElementDataContextChange(1)]
        public List<GridViewColumn> Columns
        {
            get { return (List<GridViewColumn>)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }
        public static readonly DotvvmProperty ColumnsProperty =
            DotvvmProperty.Register<List<GridViewColumn>, GridView>(c => c.Columns);

        /// <summary>
        /// Gets or sets a list of decorators that will be applied on each row.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        [ControlPropertyBindingDataContextChange("DataSource")]
        [CollectionElementDataContextChange(1)]
        public List<Decorator> RowDecorators
        {
            get { return (List<Decorator>)GetValue(RowDecoratorsProperty); }
            set { SetValue(RowDecoratorsProperty, value); }
        }
        public static readonly DotvvmProperty RowDecoratorsProperty =
            DotvvmProperty.Register<List<Decorator>, GridView>(c => c.RowDecorators);


        /// <summary>
        /// Gets or sets the command that will be triggered when the user changed the sort order.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public Action<string> SortChanged
        {
            get { return (Action<string>)GetValue(SortChangedProperty); }
            set { SetValue(SortChangedProperty, value); }
        }
        public static readonly DotvvmProperty SortChangedProperty =
            DotvvmProperty.Register<Action<string>, GridView>(c => c.SortChanged, null);

        public bool ShowHeaderWhenNoData
        {
            get { return (bool)GetValue(ShowHeaderWhenNoDataProperty); }
            set { SetValue(ShowHeaderWhenNoDataProperty, value); }
        }
        public static readonly DotvvmProperty ShowHeaderWhenNoDataProperty =
            DotvvmProperty.Register<bool, GridView>(t => t.ShowHeaderWhenNoData, false);

        public bool InlineEditing
        {
            get { return (bool)GetValue(InlineEditingProperty); }
            set { SetValue(InlineEditingProperty, value); }
        }

        public static readonly DotvvmProperty InlineEditingProperty =
            DotvvmProperty.Register<bool, GridView>(t => t.InlineEditing, false);

        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            EnsureControlHasId();

            DataBind(context);
            base.OnLoad(context);
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            DataBind(context);     // TODO: support for observable collection
            base.OnPreRender(context);
        }


        private void DataBind(IDotvvmRequestContext context)
        {
            Children.Clear();
            emptyDataContainer = null;
            head = null;

            var dataSourceBinding = GetDataSourceBinding();
            var dataSource = DataSource;

            Action<string> sortCommand = null;
            var set = dataSource as IGridViewDataSet;
            if (set != null)
            {
                sortCommand = set.SetSortExpression;
            }
            else
            {
                sortCommand = SortChanged;
            }

            // WORKAROUND: DataSource is null => don't throw exception
            if (sortCommand == null && dataSource == null)
            {
                sortCommand = s => { throw new Exception("can't sort null data source"); };
            }

            CreateHeaderRow(context, sortCommand);
            var index = 0;
            if (dataSource != null)
            {
                // create header row
                var items = GetIEnumerableFromDataSource(dataSource);
                var javascriptDataSourceExpression = dataSourceBinding.GetKnockoutBindingExpression();

                foreach (var item in items)
                {
                    // create row
                    var placeholder = new DataItemContainer { DataItemIndex = index };
                    placeholder.SetBinding(DataContextProperty, GetItemBinding((IList)items, javascriptDataSourceExpression, index));
                    placeholder.SetValue(Internal.PathFragmentProperty, JavascriptCompilationHelper.AddIndexerToViewModel(GetPathFragmentExpression(), index));
                    placeholder.ID = "i" + index;
                    CreateRow(context, placeholder);
                    Children.Add(placeholder);

                    index++;
                }
                numberOfRows = index;
            }
            else
            {
                numberOfRows = 0;
            }

            // add empty item
            if (EmptyDataTemplate != null)
            {
                emptyDataContainer = new EmptyData();
                emptyDataContainer.SetBinding(DataSourceProperty, dataSourceBinding);
                EmptyDataTemplate.BuildContent(context, emptyDataContainer);
                Children.Add(emptyDataContainer);
            }
        }

        private void CreateHeaderRow(IDotvvmRequestContext context, Action<string> sortCommand)
        {
            head = new HtmlGenericControl("thead");

            if (!ShowHeaderWhenNoData)
            {
                head.Attributes["data-bind"] = "visible: $data";
            }
            Children.Add(head);

            var gridViewDataSet = DataSource as IGridViewDataSet;
            
            // workaroud: header template must have to be one level nested, because it is in the Columns property which nests the dataContext to the item type
            // on server we need null, to be Convertible to Item type and on client the best is empty object, because with will hide the inner content when it is null
            var headerRow = new HtmlGenericControl("tr");
            headerRow.SetBinding(DataContextProperty, new ValueBindingExpression(h => null, "{}"));
            head.Children.Add(headerRow);
            foreach (var column in Columns)
            {
                var cell = new HtmlGenericControl("th");
                SetCellAttributes(column, cell, true);
                headerRow.Children.Add(cell);

                column.CreateHeaderControls(context, this, sortCommand, cell, gridViewDataSet);
                if (FilterPlacement == GridViewFilterPlacement.HeaderRow)
                {
                    column.CreateFilterControls(context, this, cell, gridViewDataSet);
                }
            }

            if (FilterPlacement == GridViewFilterPlacement.ExtraRow)
            {
                headerRow = new HtmlGenericControl("tr");
                headerRow.SetBinding(DataContextProperty, new ValueBindingExpression(h => null, "{}"));
                head.Children.Add(headerRow);
                foreach (var column in Columns)
                {
                    var cell = new HtmlGenericControl("th");
                    SetCellAttributes(column, cell, true);
                    headerRow.Children.Add(cell);
                    column.CreateFilterControls(context, this, cell, gridViewDataSet);
                }
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
                    cell.Attributes["class"] = cssClassBinding;
                }
                else if (!string.IsNullOrWhiteSpace(column.CssClass))
                {
                    cell.Attributes["class"] = column.CssClass;
                }
            }
            else
            {
                var cssClassBinding = column.GetValueBinding(GridViewColumn.HeaderCssClassProperty);
                if (cssClassBinding != null)
                {
                    cell.Attributes["class"] = cssClassBinding;
                }
                else if (!string.IsNullOrWhiteSpace(column.HeaderCssClass))
                {
                    cell.Attributes["class"] = column.HeaderCssClass;
                }
            }
        }

        private void CreateRow(IDotvvmRequestContext context, DataItemContainer placeholder)
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

            var isEdit = false;
            if (InlineEditing == true)
            {
                // if gridviewdataset is missing throw exception
                if (!(DataSource is IGridViewDataSet))
                {
                    throw new ArgumentException("You have to use GridViewDataSet with InlineEditing enabled.");
                }

                //checks if row is being edited
                isEdit = IsEditedRow(placeholder);
            }

            // create cells
            foreach (var column in Columns)
            {
                var cell = new HtmlGenericControl("td");
                SetCellAttributes(column, cell, false);
                row.Children.Add(cell);

                if (isEdit && column.IsEditable)
                {
                    column.CreateEditControls(context, cell);
                }
                else
                {
                    column.CreateControls(context, cell);
                }

            }
        }

        private bool IsEditedRow(DataItemContainer placeholder)
        {
            PropertyInfo prop;
            var value = ReflectionUtils.GetObjectPropertyValue(placeholder.DataContext, ((IGridViewDataSet)DataSource).PrimaryKeyPropertyName, out prop);

            if (value != null)
            {
                var editRowId = ((IGridViewDataSet)DataSource).EditRowId;
                if (editRowId != null && value.Equals(ReflectionUtils.ConvertValue(editRowId, prop.PropertyType)))
                {
                    return true;
                }
            }

            return false;
        }

        private void CreateTemplates(IDotvvmRequestContext context, DataItemContainer placeholder, bool createEditTemplates = false)
        {
            var row = new HtmlGenericControl("tr");

            DotvvmControl container = row;
            placeholder.Children.Add(container);


            // create cells
            foreach (var column in Columns)
            {
                var cell = new HtmlGenericControl("td");
                row.Children.Add(cell);
                if(createEditTemplates && column.IsEditable)
                {
                    column.CreateEditControls(context, cell);
                }
                else
                {
                    column.CreateControls(context, cell);
                }
                
            }
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            // render the header
            head?.Render(writer, context);

            // render body
            if (!RenderOnServer)
            {
                writer.AddKnockoutForeachDataBind("dotvvm.evaluator.getDataSourceItems($data)");

            }
            writer.RenderBeginTag("tbody");
           
        
            // render contents
            if (RenderOnServer)
            {
                // render on server
                var index = 0;
                foreach (var child in Children.Except(new[] { head, emptyDataContainer }))
                {
                    child.Render(writer, context);
                    index++;
                }
            }
            else
            {
                // render on client
                if (InlineEditing)
                {    
                    var placeholder = new DataItemContainer { DataContext = null };
                    placeholder.SetValue(Internal.PathFragmentProperty, JavascriptCompilationHelper.AddIndexerToViewModel(GetPathFragmentExpression(), "$index"));
                    placeholder.SetValue(Internal.ClientIDFragmentProperty, "'i' + $index()");
                    writer.WriteKnockoutDataBindComment("if", "ko.unwrap($parent.EditRowId) !== ko.unwrap($data[ko.unwrap($parent.PrimaryKeyPropertyName)])");
                    CreateTemplates(context.RequestContext, placeholder);
                    Children.Add(placeholder);
                    placeholder.Render(writer, context);
                    writer.WriteKnockoutDataBindEndComment();

                    var placeholderEdit = new DataItemContainer { DataContext = null };
                    placeholderEdit.SetValue(Internal.PathFragmentProperty, JavascriptCompilationHelper.AddIndexerToViewModel(GetPathFragmentExpression(), "$index"));
                    placeholderEdit.SetValue(Internal.ClientIDFragmentProperty, "'i' + $index()");
                    writer.WriteKnockoutDataBindComment("if", "ko.unwrap($parent.EditRowId) === ko.unwrap($data[ko.unwrap($parent.PrimaryKeyPropertyName)])");
                    CreateTemplates(context.RequestContext, placeholderEdit, true);
                    Children.Add(placeholderEdit);
                    placeholderEdit.Render(writer, context);
                    writer.WriteKnockoutDataBindEndComment();
                }
                else
                {
                    var placeholder = new DataItemContainer { DataContext = null };
                    placeholder.SetValue(Internal.PathFragmentProperty, JavascriptCompilationHelper.AddIndexerToViewModel(GetPathFragmentExpression(), "$index"));
                    placeholder.SetValue(Internal.ClientIDFragmentProperty, "'i' + $index()");
                    CreateRow(context.RequestContext, placeholder);
                    Children.Add(placeholder);
                    placeholder.Render(writer, context);

                }
            }

            writer.RenderEndTag();
        }

        protected override void RenderControl(IHtmlWriter writer, RenderContext context)
        {
            if (RenderOnServer && numberOfRows == 0 && !ShowHeaderWhenNoData)
            {
                emptyDataContainer?.Render(writer, context);
            }
            else
            {
                base.RenderControl(writer, context);
            }
        }

        protected override void RenderEndTag(IHtmlWriter writer, RenderContext context)
        {
            base.RenderEndTag(writer, context);

            emptyDataContainer?.Render(writer, context);
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            if (!RenderOnServer)
            {
                if (!ShowHeaderWhenNoData)
                {
                    writer.AddKnockoutDataBind("visible", $"({ GetForeachDataBindJavascriptExpression() }).length");
                    if (numberOfRows == 0)
                    {
                        writer.AddStyleAttribute("display", "none");
                    } 
                }

                // with databind
                writer.AddKnockoutDataBind("with", GetDataSourceBinding());
            }

            base.AddAttributesToRender(writer, context);
        }

        public override IEnumerable<DotvvmBindableObject> GetLogicalChildren()
        {
            return base.GetLogicalChildren().Concat(Columns).Concat(RowDecorators);
        }
    }
}
