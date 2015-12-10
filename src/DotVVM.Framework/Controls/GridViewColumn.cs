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



        [MarkupOptions(AllowBinding = false)]
        public string SortExpression
        {
            get { return (string)GetValue(SortExpressionProperty); }
            set { SetValue(SortExpressionProperty, value); }
        }
        public static readonly DotvvmProperty SortExpressionProperty =
            DotvvmProperty.Register<string, GridViewColumn>(c => c.SortExpression);


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

        public virtual void CreateHeaderControls(IDotvvmRequestContext context, GridView gridView, Action<string> sortCommand, HtmlGenericControl cell)
        {
            if (AllowSorting)
            {
                if (sortCommand == null)
                {
                    throw new DotvvmControlException(this, "Cannot use column sorting where no sort command is specified. Either put IGridViewDataSet in the DataSource property of the GridView, or set the SortChanged command on the GridView to implement custom sorting logic!");
                }

                var sortExpression = GetSortExpression();

                var linkButton = new LinkButton();
                if (HeaderTemplate != null) HeaderTemplate.BuildContent(context, linkButton);
                else linkButton.Text = HeaderText;
                cell.Children.Add(linkButton);
                var bindingId = linkButton.GetValue(Internal.UniqueIDProperty) + "_sortBinding";
                var binding = new CommandBindingExpression(h => sortCommand(sortExpression), bindingId);
                linkButton.SetBinding(ButtonBase.ClickProperty, binding);
            }
            else
            {
                if (HeaderTemplate == null)
                {
                    var literal = new Literal(HeaderText);
                    cell.Children.Add(literal);
                }
                else HeaderTemplate.BuildContent(context, cell);
            }
        }

        protected virtual string GetSortExpression()
        {
            // TODO: verify that sortExpression is a single property name
            return SortExpression;
        }
    }

}