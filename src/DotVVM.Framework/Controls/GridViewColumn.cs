using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using System;

namespace DotVVM.Framework.Controls
{
    public abstract class GridViewColumn : DotvvmBindableControl
    {

        [MarkupOptions(AllowBinding = false)]
        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }
        public static readonly DotvvmProperty HeaderTextProperty =
            DotvvmProperty.Register<string, GridViewColumn>(c => c.HeaderText);

        [MarkupOptions(AllowHardCodedValue = false)]
        public object ValueBinding
        {
            get { return GetValue(ValueBindingProperty); }
            set { SetValue(ValueBindingProperty, value); }
        }
        public static readonly DotvvmProperty ValueBindingProperty =
            DotvvmProperty.Register<object, GridViewColumn>(c => c.ValueBinding);


        [MarkupOptions(AllowBinding = false)]
        public string SortExpression
        {
            get { return (string)GetValue(SortExpressionProperty); }
            set { SetValue(SortExpressionProperty, value); }
        }
        public static readonly DotvvmProperty SortExpressionProperty =
            DotvvmProperty.Register<string, GridViewColumn>(c => c.SortExpression);


        [MarkupOptions(AllowBinding = false)]
        public bool AllowSorting
        {
            get { return (bool)GetValue(AllowSortingProperty); }
            set { SetValue(AllowSortingProperty, value); }
        }
        public static readonly DotvvmProperty AllowSortingProperty =
            DotvvmProperty.Register<bool, GridViewColumn>(c => c.AllowSorting, false);


        public string CssClass
        {
            get { return (string)GetValue(CssClassProperty); }
            set { SetValue(CssClassProperty, value); }
        }
        public static readonly DotvvmProperty CssClassProperty =
            DotvvmProperty.Register<string, GridViewColumn>(c => c.CssClass);


        public string HeaderCssClass
        {
            get { return (string)GetValue(HeaderCssClassProperty); }
            set { SetValue(HeaderCssClassProperty, value); }
        }
        public static readonly DotvvmProperty HeaderCssClassProperty =
            DotvvmProperty.Register<string, GridViewColumn>(c => c.HeaderCssClass);

        [MarkupOptions(AllowBinding = false)]
        public string Width
        {
            get { return (string)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }
        public static readonly DotvvmProperty WidthProperty =
            DotvvmProperty.Register<string, GridViewColumn>(c => c.Width);






        public abstract void CreateControls(IDotvvmRequestContext context, DotvvmControl container);


        private Exception ValueBindingNotSet()
        {
            return new Exception(string.Format("The ValueBinding property is not set on the {0} control!", GetType()));
        }


        public virtual void CreateHeaderControls(IDotvvmRequestContext context, GridView gridView, Action<string> sortCommand, HtmlGenericControl cell)
        {
            if (AllowSorting)
            {
                if (sortCommand == null)
                {
                    throw new Exception("Cannot use column sorting where no sort command is specified. Either put IGridViewDataSet in the DataSource property of the GridView, or set the SortChanged command on the GridView to implement custom sorting logic!");
                }

                var sortExpression = GetSortExpression();

                var linkButton = new LinkButton() { Text = HeaderText };
                cell.Children.Add(linkButton);
                var bindingId = linkButton.GetValue(Internal.UniqueIDProperty) + "_sortBinding";
                var binding = new CommandBindingExpression((o, i) => { sortCommand(sortExpression); return null; }, bindingId);
                linkButton.SetBinding(ButtonBase.ClickProperty, binding);
            }
            else
            {
                var literal = new Literal(HeaderText);
                cell.Children.Add(literal);
            }
        }

        private string GetSortExpression()
        {
            // TODO: verify that sortExpression is a single property name
            if (string.IsNullOrEmpty(SortExpression))
            {
                var valueBinding = GetValueBinding(ValueBindingProperty) as ValueBindingExpression;
                if (valueBinding != null)
                {
                    return valueBinding.OriginalString;
                }
                else
                {
                    throw ValueBindingNotSet();
                }
            }
            else
            {
                return SortExpression;
            }
        }
    }

}