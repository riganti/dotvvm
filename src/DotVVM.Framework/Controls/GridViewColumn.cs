using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using System;
using DotVVM.Framework.Exceptions;

namespace DotVVM.Framework.Controls
{
    public abstract class GridViewColumn : DotvvmBindableObject
    {

        [MarkupOptions(AllowBinding = false)]
        public string HeaderText { get; set; }

        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate HeaderTemplate { get; set; }

        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate FilterTemplate { get; set; }

        [MarkupOptions(AllowBinding = false)]
        public string SortExpression
        {
            get { return (string)GetValue(SortExpressionProperty); }
            set { SetValue(SortExpressionProperty, value); }
        }
        public static readonly DotvvmProperty SortExpressionProperty =
            DotvvmProperty.Register<string, GridViewColumn>(c => c.SortExpression);


        [MarkupOptions(AllowBinding = false)]
        public string SortAscendingHeaderCssClass
        {
            get { return (string)GetValue(SortUpCssClassProperty); }
            set { SetValue(SortUpCssClassProperty, value); }
        }
        public static readonly DotvvmProperty SortUpCssClassProperty =
            DotvvmProperty.Register<string, GridViewColumn>(c => c.SortAscendingHeaderCssClass, "sort-asc");


        [MarkupOptions(AllowBinding = false)]
        public string SortDescendingHeaderCssClass
        {
            get { return (string)GetValue(SortDownCssClassProperty); }
            set { SetValue(SortDownCssClassProperty, value); }
        }
        public static readonly DotvvmProperty SortDownCssClassProperty =
            DotvvmProperty.Register<string, GridViewColumn>(c => c.SortDescendingHeaderCssClass, "sort-desc");

        [MarkupOptions(AllowBinding = false)]
        public bool AllowSorting { get; set; }

        public string CssClass
        {
            get { return (string)GetValue(CssClassProperty); }
            set { SetValue(CssClassProperty, value); }
        }
        public static readonly DotvvmProperty CssClassProperty =
            DotvvmProperty.Register<string, GridViewColumn>(c => c.CssClass);

        [MarkupOptions(AllowBinding = false)]
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        public static readonly DotvvmProperty IsEditableProperty =
            DotvvmProperty.Register<bool, GridViewColumn>(t => t.IsEditable, true);



        public string HeaderCssClass
        {
            get { return (string)GetValue(HeaderCssClassProperty); }
            set { SetValue(HeaderCssClassProperty, value); }
        }
        public static readonly DotvvmProperty HeaderCssClassProperty =
            DotvvmProperty.Register<string, GridViewColumn>(c => c.HeaderCssClass);

        [MarkupOptions(AllowBinding = false)]
        public string Width { get; set; }


        public abstract void CreateControls(IDotvvmRequestContext context, DotvvmControl container);

        public abstract void CreateEditControls(IDotvvmRequestContext context, DotvvmControl container);

        public virtual void CreateHeaderControls(IDotvvmRequestContext context, GridView gridView, Action<string> sortCommand, HtmlGenericControl cell, IGridViewDataSet gridViewDataSet)
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
                linkButton.Text = HeaderText;
                cell.Children.Add(linkButton);

                var bindingId = linkButton.GetValue(Internal.UniqueIDProperty) + "_sortBinding";
                var binding = new CommandBindingExpression(h => sortCommand(sortExpression), bindingId);
                linkButton.SetBinding(ButtonBase.ClickProperty, binding);

                SetSortedCssClass(cell, gridViewDataSet);
            }
            else
            {
                var literal = new Literal(HeaderText);
                cell.Children.Add(literal);
            }
        }

        public virtual void CreateFilterControls(IDotvvmRequestContext context, GridView gridView, HtmlGenericControl cell, IGridViewDataSet gridViewDataSet)
        {
            if (FilterTemplate != null)
            {
                var placeholder = new PlaceHolder();
                cell.Children.Add(placeholder);
                FilterTemplate.BuildContent(context, placeholder);
            }
        }

        private void SetSortedCssClass(HtmlGenericControl cell, IGridViewDataSet gridViewDataSet)
        {
            if (RenderOnServer)
            {
                if (gridViewDataSet != null)
                {
                    if (gridViewDataSet.SortExpression == GetSortExpression())
                    {
                        if (gridViewDataSet.SortDescending)
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
            else
            {
                cell.Attributes["data-bind"] =
                $"css: {{ '{SortDescendingHeaderCssClass}': ko.unwrap($parent.SortExpression) == '{GetSortExpression()}' && $parent.SortDescending(), '{SortAscendingHeaderCssClass}': ko.unwrap($parent.SortExpression) == '{GetSortExpression()}' && !$parent.SortDescending()}}";
            }
        }

        protected virtual string GetSortExpression()
        {
            // TODO: verify that sortExpression is a single property name
            return SortExpression;
        }
    }

}