using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using System.Collections;
using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.ViewModel;
using Newtonsoft.Json;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A multi-purpose grid control with advanced binding, templating options and sorting support.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = nameof(Columns))]
    public class GridView : ItemsControl
    {
        private EmptyData? emptyDataContainer;
        private int numberOfRows;
        private HtmlGenericControl? head;

        public GridView() : base("table")
        {
            SetValue(Internal.IsNamingContainerProperty, true);

            Columns = new List<GridViewColumn>();
            RowDecorators = new List<Decorator>();

            if (GetType() == typeof(GridView))
                LifecycleRequirements &= ~(ControlLifecycleRequirements.InvokeMissingInit | ControlLifecycleRequirements.InvokeMissingLoad);
        }


        /// <summary>
        /// Gets or sets the place where the filters will be created.
        /// </summary>
        public GridViewFilterPlacement FilterPlacement
        {
            get { return (GridViewFilterPlacement)GetValue(FilterPlacementProperty)!; }
            set { SetValue(FilterPlacementProperty, value); }
        }
        public static readonly DotvvmProperty FilterPlacementProperty
            = DotvvmProperty.Register<GridViewFilterPlacement, GridView>(c => c.FilterPlacement, GridViewFilterPlacement.HeaderRow);


        /// <summary>
        /// Gets or sets the template which will be displayed when the DataSource is empty.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate? EmptyDataTemplate
        {
            get { return (ITemplate?)GetValue(EmptyDataTemplateProperty); }
            set { SetValue(EmptyDataTemplateProperty, value); }
        }
        public static readonly DotvvmProperty EmptyDataTemplateProperty =
            DotvvmProperty.Register<ITemplate?, GridView>(t => t.EmptyDataTemplate, null);

        /// <summary>
        /// Gets or sets a collection of columns that will be placed inside the grid.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        [ControlPropertyBindingDataContextChange("DataSource")]
        [CollectionElementDataContextChange(1)]
        public List<GridViewColumn>? Columns
        {
            get { return (List<GridViewColumn>?)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }
        public static readonly DotvvmProperty ColumnsProperty =
            DotvvmProperty.Register<List<GridViewColumn>, GridView>(c => c.Columns);

        /// <summary>
        /// Gets or sets a list of decorators that will be applied on each row which is not in the edit mode.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        [ControlPropertyBindingDataContextChange("DataSource")]
        [CollectionElementDataContextChange(1)]
        public List<Decorator>? RowDecorators
        {
            get { return (List<Decorator>?)GetValue(RowDecoratorsProperty); }
            set { SetValue(RowDecoratorsProperty, value); }
        }

        public static readonly DotvvmProperty RowDecoratorsProperty =
            DotvvmProperty.Register<List<Decorator>?, GridView>(c => c.RowDecorators);

        /// <summary>
        /// Gets or sets a list of decorators that will be applied on the header row.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public List<Decorator>? HeaderRowDecorators
        {
            get { return (List<Decorator>?)GetValue(HeaderRowDecoratorsProperty); }
            set { SetValue(HeaderRowDecoratorsProperty, value); }
        }

        public static readonly DotvvmProperty HeaderRowDecoratorsProperty =
            DotvvmProperty.Register<List<Decorator>?, GridView>(c => c.HeaderRowDecorators);

        /// <summary>
        /// Gets or sets a list of decorators that will be applied on each row in edit mode.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        [ControlPropertyBindingDataContextChange("DataSource")]
        [CollectionElementDataContextChange(1)]
        public List<Decorator>? EditRowDecorators
        {
            get { return (List<Decorator>?)GetValue(EditRowDecoratorsProperty); }
            set { SetValue(EditRowDecoratorsProperty, value); }
        }
        public static readonly DotvvmProperty EditRowDecoratorsProperty =
            DotvvmProperty.Register<List<Decorator>?, GridView>(c => c.EditRowDecorators);


        /// <summary>
        /// Gets or sets the command that will be triggered when the user changed the sort order.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public Action<string?>? SortChanged
        {
            get { return (Action<string?>?)GetValue(SortChangedProperty); }
            set { SetValue(SortChangedProperty, value); }
        }
        public static readonly DotvvmProperty SortChangedProperty =
            DotvvmProperty.Register<Action<string>?, GridView>(c => c.SortChanged, null);

        /// <summary>
        /// Gets or sets whether the header row should be displayed when the grid is empty.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool ShowHeaderWhenNoData
        {
            get { return (bool)GetValue(ShowHeaderWhenNoDataProperty)!; }
            set { SetValue(ShowHeaderWhenNoDataProperty, value); }
        }
        public static readonly DotvvmProperty ShowHeaderWhenNoDataProperty =
            DotvvmProperty.Register<bool, GridView>(t => t.ShowHeaderWhenNoData, false);

        /// <summary>
        /// Gets or sets whether the inline editing is allowed in the Grid. If so, you have to use a GridViewDataSet as the DataSource.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool InlineEditing
        {
            get { return (bool)GetValue(InlineEditingProperty)!; }
            set { SetValue(InlineEditingProperty, value); }
        }

        public static readonly DotvvmProperty InlineEditingProperty =
            DotvvmProperty.Register<bool, GridView>(t => t.InlineEditing, false);

        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
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

            var sortCommand = SortChanged;
            sortCommand ??=
                typeof(ISortableGridViewDataSet).IsAssignableFrom((GetBinding(DataSourceProperty) as IStaticValueBinding)?.ResultType)
                  ? (Action<string?>)SortChangedCommand
                  : null;

            sortCommand ??= s =>
                throw new DotvvmControlException(this, "Cannot sort when DataSource is null.");

            CreateHeaderRow(context, sortCommand);

            var index = 0;
            if (dataSource != null)
            {
                foreach (var item in GetIEnumerableFromDataSource()!)
                {
                    // create row
                    var placeholder = new DataItemContainer { DataItemIndex = index };
                    placeholder.SetDataContextTypeFromDataSource(dataSourceBinding);
                    placeholder.DataContext = item;
                    placeholder.SetValue(Internal.PathFragmentProperty, GetPathFragmentExpression() + "/[" + index + "]");
                    placeholder.ID = index.ToString();
                    Children.Add(placeholder);
                    CreateRowWithCells(context, placeholder);

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
                emptyDataContainer.SetValue(EmptyData.VisibleProperty, GetValueRaw(VisibleProperty));
                emptyDataContainer.SetBinding(DataSourceProperty, dataSourceBinding);
                EmptyDataTemplate.BuildContent(context, emptyDataContainer);
                Children.Add(emptyDataContainer);
            }
        }

        protected virtual void SortChangedCommand(string? expr)
        {
            var dataSource = this.DataSource;
            if (dataSource is null)
                throw new DotvvmControlException(this, "Cannot execute sort command, DataSource is null");
            var sortOptions = (dataSource as ISortableGridViewDataSet)?.SortingOptions;
            if (sortOptions is null)
                throw new DotvvmControlException(this, "Cannot execute sort command, DataSource does not have sorting options");
            if (sortOptions.SortExpression == expr)
            {
                sortOptions.SortDescending ^= true;
            }
            else
            {
                sortOptions.SortExpression = expr;
                sortOptions.SortDescending = false;
            }
            (dataSource as IPageableGridViewDataSet)?.GoToFirstPage();
        }

        protected virtual void CreateHeaderRow(IDotvvmRequestContext context, Action<string?>? sortCommand)
        {
            head = new HtmlGenericControl("thead");
            Children.Add(head);

            var gridViewDataSet = DataSource as IGridViewDataSet;

            var headerRow = new HtmlGenericControl("tr");
            var decoratedHeaderRow = Decorator.ApplyDecorators(headerRow, HeaderRowDecorators);
            head.Children.Add(decoratedHeaderRow);

            foreach (var column in Columns.NotNull("GridView.Columns must be set"))
            {
                var cell = new HtmlGenericControl("th");
                SetCellAttributes(column, cell, true);
                var decoratedCell = Decorator.ApplyDecorators(cell, column.HeaderCellDecorators);
                headerRow.Children.Add(decoratedCell);

                column.CreateHeaderControls(context, this, sortCommand, cell, gridViewDataSet);
                if (FilterPlacement == GridViewFilterPlacement.HeaderRow)
                {
                    column.CreateFilterControls(context, this, cell, gridViewDataSet);
                }
            }

            if (FilterPlacement == GridViewFilterPlacement.ExtraRow)
            {
                headerRow = new HtmlGenericControl("tr");
                head.Children.Add(headerRow);
                foreach (var column in Columns.NotNull("GridView.Columns must be set"))
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
            var cellAttributes = cell.Attributes;
            if (!string.IsNullOrEmpty(column.Width))
            {
                cellAttributes["style"] = "width: " + column.Width;
            }

            if (!isHeaderCell)
            {
                var cssClassBinding = column.GetValueBinding(GridViewColumn.CssClassProperty);
                if (cssClassBinding != null)
                {
                    cellAttributes["class"] = cssClassBinding;
                }
                else if (!string.IsNullOrWhiteSpace(column.CssClass))
                {
                    cellAttributes["class"] = column.CssClass;
                }
            }
            else
            {
                if (column.IsPropertySet(GridViewColumn.VisibleProperty)) cell.SetValue(TableUtils.ColumnVisibleProperty, GridViewColumn.VisibleProperty.GetValue(column));
                if (column.IsPropertySet(GridViewColumn.HeaderCssClassProperty)) // transfer all bindings (even StaticValue), because column has wrong DataContext for them
                {
                    cellAttributes["class"] = column.GetValueRaw(GridViewColumn.HeaderCssClassProperty);
                }
            }
        }

        private void CreateRowWithCells(IDotvvmRequestContext context, DataItemContainer placeholder)
        {
            var isInEditMode = false;
            if (InlineEditing)
            {
                // if gridviewdataset is missing throw exception
                if (!(DataSource is IGridViewDataSet))
                {
                    throw new ArgumentException("You have to use GridViewDataSet with InlineEditing enabled.");
                }

                //checks if row is being edited
                isInEditMode = IsEditedRow(placeholder);
            }

            var row = CreateRow(placeholder, isInEditMode);

            // create cells
            foreach (var column in Columns.NotNull("GridView.Columns must be set"))
            {
                var editMode = isInEditMode && column.IsEditable;

                var cell = new HtmlGenericControl("td");
                cell.SetDataContextType(column.GetDataContextType());
                SetCellAttributes(column, cell, false);
                var decoratedCell = Decorator.ApplyDecorators(cell, editMode ? column.EditCellDecorators : column.CellDecorators);
                row.Children.Add(decoratedCell);

                if (editMode)
                {
                    column.CreateEditControls(context, cell);
                }
                else
                {
                    column.CreateControls(context, cell);
                }
            }
        }

        private HtmlGenericControl CreateRow(DataItemContainer placeholder, bool isInEditMode)
        {
            var row = new HtmlGenericControl("tr");

            var decoratedRow = Decorator.ApplyDecorators(row, isInEditMode ? EditRowDecorators : RowDecorators);
            placeholder.Children.Add(decoratedRow);

            return row;
        }

        private PropertyInfo ResolvePrimaryKeyProperty()
        {
            var dataSet = (IGridViewDataSet)DataSource.NotNull();
            var primaryKeyPropertyName = dataSet.RowEditOptions.PrimaryKeyPropertyName;
            if (string.IsNullOrEmpty(primaryKeyPropertyName))
            {
                throw new DotvvmControlException(this, $"The {nameof(IGridViewDataSet)} must " +
                    $"specify the {nameof(IRowEditOptions.PrimaryKeyPropertyName)} property " +
                    $"when inline editing is enabled on the {nameof(GridView)} control!");
            }

            var enumerableType = ReflectionUtils.GetEnumerableType(dataSet.Items.GetType())!;
            var property = enumerableType.GetProperty(primaryKeyPropertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"Type '{enumerableType}' does not contain a " +
                    $"'{primaryKeyPropertyName}' property.");
            }
            return property;
        }

        private bool IsEditedRow(DataItemContainer placeholder)
        {
            var property = ResolvePrimaryKeyProperty();
            var value = property.GetValue(placeholder.DataContext);
            if (value != null)
            {
                var editRowId = ((IGridViewDataSet)DataSource!).RowEditOptions.EditRowId;
                if (editRowId != null && value.Equals(ReflectionUtils.ConvertValue(editRowId, property.PropertyType)))
                {
                    return true;
                }
            }

            return false;
        }

        private void CreateTemplates(IDotvvmRequestContext context, DataItemContainer placeholder, bool isInEditMode = false)
        {
            var row = CreateRow(placeholder, isInEditMode);

            // create cells
            foreach (var column in Columns.NotNull("GridView.Columns must be set"))
            {
                var editMode = isInEditMode && column.IsEditable;

                var cell = new HtmlGenericControl("td");
                cell.SetDataContextType(column.GetDataContextType());
                SetCellAttributes(column, cell, false);
                var decoratedCell = Decorator.ApplyDecorators(cell, editMode ? column.EditCellDecorators : column.CellDecorators);
                row.Children.Add(decoratedCell);

                if (editMode)
                {
                    column.CreateEditControls(context, cell);
                }
                else
                {
                    column.CreateControls(context, cell);
                }
            }
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // render the header
            head?.Render(writer, context);

            // render body
            var foreachBinding = GetForeachDataBindExpression().GetKnockoutBindingExpression(this);
            if (RenderOnServer)
            {
                writer.AddKnockoutDataBind("dotvvm-SSR-foreach", "{data:" + foreachBinding + "}");
            }
            else
            {
                writer.AddKnockoutForeachDataBind(foreachBinding);
            }
            writer.RenderBeginTag("tbody");

            // render contents
            if (RenderOnServer)
            {
                // render on server
                var index = 0;
                foreach (var child in Children.Except(new[] { head!, emptyDataContainer! }))
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
                    placeholder.SetDataContextTypeFromDataSource(GetBinding(DataSourceProperty).NotNull());
                    placeholder.SetValue(Internal.PathFragmentProperty, GetPathFragmentExpression() + "/[$index]");
                    placeholder.SetValue(Internal.ClientIDFragmentProperty, this.GetIndexBinding(context));
                    writer.WriteKnockoutDataBindComment("if", "!$gridViewDataSetHelper.isInEditMode($context)");
                    CreateTemplates(context, placeholder);
                    Children.Add(placeholder);
                    placeholder.Render(writer, context);
                    writer.WriteKnockoutDataBindEndComment();

                    var placeholderEdit = new DataItemContainer { DataContext = null };
                    placeholderEdit.SetDataContextTypeFromDataSource(GetBinding(DataSourceProperty).NotNull());
                    placeholderEdit.SetValue(Internal.PathFragmentProperty, GetPathFragmentExpression() + "/[$index]");
                    placeholderEdit.SetValue(Internal.ClientIDFragmentProperty, this.GetIndexBinding(context));
                    writer.WriteKnockoutDataBindComment("if", "$gridViewDataSetHelper.isInEditMode($context)");
                    CreateTemplates(context, placeholderEdit, true);
                    Children.Add(placeholderEdit);
                    placeholderEdit.Render(writer, context);
                    writer.WriteKnockoutDataBindEndComment();
                }
                else
                {
                    var placeholder = new DataItemContainer { DataContext = null };
                    placeholder.SetDataContextTypeFromDataSource(GetBinding(DataSourceProperty).NotNull());
                    placeholder.SetValue(Internal.PathFragmentProperty, GetPathFragmentExpression() + "/[$index]");
                    placeholder.SetValue(Internal.ClientIDFragmentProperty, this.GetIndexBinding(context));
                    Children.Add(placeholder);
                    CreateRowWithCells(context, placeholder);
                    placeholder.Render(writer, context);

                }
            }

            writer.RenderEndTag();
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (!ShowHeaderWhenNoData)
            {
                writer.WriteKnockoutDataBindComment("if",
                    GetForeachDataBindExpression().GetProperty<DataSourceLengthBinding>().Binding.CastTo<IValueBinding>().GetKnockoutBindingExpression(this));
            }

            base.RenderBeginTag(writer, context);
        }

        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
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

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.RenderEndTag(writer, context);

            if (!ShowHeaderWhenNoData)
            {
                writer.WriteKnockoutDataBindEndComment();
            }

            emptyDataContainer?.Render(writer, context);
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var itemType = ReflectionUtils.GetEnumerableType(GetDataSourceBinding().ResultType);
            var userColumnMappingService = context.Services.GetRequiredService<UserColumnMappingCache>();
            var mapping = userColumnMappingService.GetMapping(itemType!);
            var mappingJson = JsonConvert.SerializeObject(mapping);

            writer.AddKnockoutDataBind("dotvvm-gridviewdataset", $"{{'mapping':{mappingJson},'dataSet':{GetDataSourceBinding().GetKnockoutBindingExpression(this, unwrapped: true)}}}");
            base.AddAttributesToRender(writer, context);
        }



        public override IEnumerable<DotvvmBindableObject> GetLogicalChildren()
        {
            return base.GetLogicalChildren().Concat(
                Columns ?? Enumerable.Empty<GridViewColumn>()
            ).Concat(
                RowDecorators ?? Enumerable.Empty<Decorator>()
            );
        }
    }
}
