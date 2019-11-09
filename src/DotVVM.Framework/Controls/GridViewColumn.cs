#nullable enable
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using System;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using System.Threading.Tasks;
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

                SetSortedCssClass(cell, gridViewDataSet);
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

        private void SetSortedCssClass(HtmlGenericControl cell, ISortableGridViewDataSet? sortableGridViewDataSet)
        {
            if (sortableGridViewDataSet != null)
            {
                if (!RenderOnServer)
                {
                    cell.Attributes["data-bind"] = $"css: {{ '{SortDescendingHeaderCssClass}': ko.unwrap(ko.unwrap($gridViewDataSet).SortingOptions().SortExpression) == '{GetSortExpression()}' && ko.unwrap(ko.unwrap($gridViewDataSet).SortingOptions().SortDescending), '{SortAscendingHeaderCssClass}': ko.unwrap(ko.unwrap($gridViewDataSet).SortingOptions().SortExpression) == '{GetSortExpression()}' && !ko.unwrap(ko.unwrap($gridViewDataSet).SortingOptions().SortDescending)}}";
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
    }

}
