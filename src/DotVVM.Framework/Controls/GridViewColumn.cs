#nullable enable
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Controls
{
    public abstract class GridViewColumn : DotvvmBindableObject
    {
        [PopDataContextManipulation]
        public string? HeaderText
        {
            get { return (string?)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }
        public static readonly DotvvmProperty HeaderTextProperty
            = DotvvmProperty.Register<string?, GridViewColumn>(c => c.HeaderText, null);

        [PopDataContextManipulation]
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate? HeaderTemplate
        {
            get { return (ITemplate?)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }
        public static readonly DotvvmProperty HeaderTemplateProperty
            = DotvvmProperty.Register<ITemplate?, GridViewColumn>(c => c.HeaderTemplate, null);

        [PopDataContextManipulation]
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate? FilterTemplate
        {
            get { return (ITemplate?)GetValue(FilterTemplateProperty); }
            set { SetValue(FilterTemplateProperty, value); }
        }
        public static readonly DotvvmProperty FilterTemplateProperty
            = DotvvmProperty.Register<ITemplate?, GridViewColumn>(c => c.FilterTemplate, null);

        [PopDataContextManipulation]
        [MarkupOptions(AllowBinding = false)]
        public string? SortExpression
        {
            get { return (string?)GetValue(SortExpressionProperty); }
            set { SetValue(SortExpressionProperty, value); }
        }
        public static readonly DotvvmProperty SortExpressionProperty =
            DotvvmProperty.Register<string?, GridViewColumn>(c => c.SortExpression);

        [PopDataContextManipulation]
        [MarkupOptions(AllowBinding = false)]
        public string? SortAscendingHeaderCssClass
        {
            get { return (string?)GetValue(SortAscendingHeaderCssClassProperty); }
            set { SetValue(SortAscendingHeaderCssClassProperty, value); }
        }
        public static readonly DotvvmProperty SortAscendingHeaderCssClassProperty =
            DotvvmProperty.Register<string?, GridViewColumn>(c => c.SortAscendingHeaderCssClass, "sort-asc");

        [PopDataContextManipulation]
        [MarkupOptions(AllowBinding = false)]
        public string? SortDescendingHeaderCssClass
        {
            get { return (string?)GetValue(SortDescendingHeaderCssClassProperty); }
            set { SetValue(SortDescendingHeaderCssClassProperty, value); }
        }
        public static readonly DotvvmProperty SortDescendingHeaderCssClassProperty =
            DotvvmProperty.Register<string?, GridViewColumn>(c => c.SortDescendingHeaderCssClass, "sort-desc");

        [PopDataContextManipulation]
        [MarkupOptions(AllowBinding = false)]
        public bool AllowSorting
        {
            get { return (bool)GetValue(AllowSortingProperty)!; }
            set { SetValue(AllowSortingProperty, value); }
        }
        public static readonly DotvvmProperty AllowSortingProperty
            = DotvvmProperty.Register<bool, GridViewColumn>(c => c.AllowSorting, false);

        public string? CssClass
        {
            get { return (string?)GetValue(CssClassProperty); }
            set { SetValue(CssClassProperty, value); }
        }
        public static readonly DotvvmProperty CssClassProperty =
            DotvvmProperty.Register<string?, GridViewColumn>(c => c.CssClass);

        [PopDataContextManipulation]
        [MarkupOptions(AllowBinding = false)]
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty)!; }
            set { SetValue(IsEditableProperty, value); }
        }
        public static readonly DotvvmProperty IsEditableProperty =
            DotvvmProperty.Register<bool, GridViewColumn>(t => t.IsEditable, true);

        [PopDataContextManipulation]
        public string? HeaderCssClass
        {
            get { return (string?)GetValue(HeaderCssClassProperty); }
            set { SetValue(HeaderCssClassProperty, value); }
        }
        public static readonly DotvvmProperty HeaderCssClassProperty =
            DotvvmProperty.Register<string?, GridViewColumn>(c => c.HeaderCssClass);

        [PopDataContextManipulation]
        [MarkupOptions(AllowBinding = false)]
        public string? Width
        {
            get { return (string?)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }
        public static readonly DotvvmProperty WidthProperty
            = DotvvmProperty.Register<string?, GridViewColumn>(c => c.Width, null);

        [PopDataContextManipulation]
        [MarkupOptions(AllowHardCodedValue = false)]
        public bool Visible
        {
            get { return (bool)GetValue(VisibleProperty)!; }
            set { SetValue(VisibleProperty, value); }
        }
        public static readonly DotvvmProperty VisibleProperty
            = DotvvmProperty.Register<bool, GridViewColumn>(c => c.Visible, true);

        /// <summary>
        /// Gets or sets a list of decorators that will be applied on each cell which is not in the edit mode.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public List<Decorator>? CellDecorators
        {
            get { return (List<Decorator>?)GetValue(CellDecoratorsProperty); }
            set { SetValue(CellDecoratorsProperty, value); }
        }

        public static readonly DotvvmProperty CellDecoratorsProperty =
            DotvvmProperty.Register<List<Decorator>?, GridViewColumn>(c => c.CellDecorators);

        /// <summary>
        /// Gets or sets a list of decorators that will be applied on each cell which is in the edit mode.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public List<Decorator>? EditCellDecorators
        {
            get { return (List<Decorator>?)GetValue(EditCellDecoratorsProperty); }
            set { SetValue(EditCellDecoratorsProperty, value); }
        }

        public static readonly DotvvmProperty EditCellDecoratorsProperty =
            DotvvmProperty.Register<List<Decorator>?, GridViewColumn>(c => c.EditCellDecorators);

        /// <summary>
        /// Gets or sets a list of decorators that will be applied on each header cell.
        /// </summary>
        [PopDataContextManipulation]
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public List<Decorator>? HeaderCellDecorators
        {
            get { return (List<Decorator>?)GetValue(HeaderCellDecoratorsProperty); }
            set { SetValue(HeaderCellDecoratorsProperty, value); }
        }

        public static readonly DotvvmProperty HeaderCellDecoratorsProperty =
            DotvvmProperty.Register<List<Decorator>?, GridViewColumn>(c => c.HeaderCellDecorators);

        public abstract void CreateControls(IDotvvmRequestContext context, DotvvmControl container);

        public abstract void CreateEditControls(IDotvvmRequestContext context, DotvvmControl container);

        public virtual void CreateHeaderControls(IDotvvmRequestContext context, GridView gridView, Action<string?>? sortCommand, HtmlGenericControl cell, IGridViewDataSet? gridViewDataSet)
        {
            if (HeaderTemplate != null)
            {
                HeaderTemplate.BuildContent(context, cell);
                return;
            }

            if (AllowSorting)
            {
                if (sortCommand == null)
                {
                    throw new DotvvmControlException(this, "Cannot use column sorting where no sort command is specified. Either put IGridViewDataSet in the DataSource property of the GridView, or set the SortChanged command on the GridView to implement custom sorting logic!");
                }

                var sortExpression = GetSortExpression();

                var linkButton = new LinkButton();
                linkButton.SetValue(LinkButton.TextProperty, GetValueRaw(HeaderTextProperty));
                cell.Children.Add(linkButton);

                var bindingId = linkButton.GetDotvvmUniqueId() + "_sortBinding";
                var binding = new CommandBindingExpression(context.Services.GetRequiredService<BindingCompilationService>().WithoutInitialization(), h => sortCommand(sortExpression), bindingId);
                linkButton.SetBinding(ButtonBase.ClickProperty, binding);

                SetSortedCssClass(cell, gridViewDataSet, gridView.GetValueBinding(GridView.DataSourceProperty)!);
            }
            else
            {
                var literal = new Literal();
                literal.SetValue(Literal.TextProperty, GetValueRaw(HeaderTextProperty));
                cell.Children.Add(literal);
            }
        }

        public virtual void CreateFilterControls(IDotvvmRequestContext context, GridView gridView, HtmlGenericControl cell, ISortableGridViewDataSet? sortableGridViewDataSet)
        {
            if (FilterTemplate != null)
            {
                var placeholder = new PlaceHolder();
                cell.Children.Add(placeholder);
                FilterTemplate.BuildContent(context, placeholder);
            }
        }

        private void SetSortedCssClass(HtmlGenericControl cell, ISortableGridViewDataSet? sortableGridViewDataSet, IValueBinding dataSourceBinding)
        {
            if (sortableGridViewDataSet != null)
            {
                if (!RenderOnServer)
                {
                    var gridViewDataSetExpr = dataSourceBinding.GetKnockoutBindingExpression(cell, unwrapped: true);
                    cell.Attributes["data-bind"] = $"css: {{ '{SortDescendingHeaderCssClass}': ({gridViewDataSetExpr}).SortingOptions().SortExpression() == '{GetSortExpression()}' && ({gridViewDataSetExpr}).SortingOptions().SortDescending(), '{SortAscendingHeaderCssClass}': ({gridViewDataSetExpr}).SortingOptions().SortExpression() == '{GetSortExpression()}' && !({gridViewDataSetExpr}).SortingOptions().SortDescending()}}";
                }
                else if (sortableGridViewDataSet.SortingOptions.SortExpression == GetSortExpression())
                {
                    if (sortableGridViewDataSet.SortingOptions.SortDescending)
                    {
                        cell.Attributes["class"] = SortDescendingHeaderCssClass;
                    }
                    else
                    {
                        cell.Attributes["class"] = SortAscendingHeaderCssClass;
                    }
                }
            }
        }

        protected virtual string? GetSortExpression()
        {
            // TODO: verify that sortExpression is a single property name
            return SortExpression;
        }

        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            if (control.Properties.ContainsKey(DataContextProperty))
            {
                yield return new ControlUsageError("Changing the DataContext property on the GridViewColumn is not supported!", control.DothtmlNode);
            }

            // disallow attached properties on columns
            foreach (var property in control.Properties)
            {
                // ignore attached properties that are set by runtime and not from markup
                if (Internal.IsViewCompilerProperty(property.Key)) continue;

                if (!typeof(GridViewColumn).IsAssignableFrom(property.Key.DeclaringType))
                {
                    yield return new ControlUsageError($"The column doesn't support the property {property.Key.FullName}! If you need to set an attached property applied to a table cell, use the CellDecorators property.",
                        new[] { property.Value.DothtmlNode }.Where(n => n != null));
                }
            }
        }

    }

}
