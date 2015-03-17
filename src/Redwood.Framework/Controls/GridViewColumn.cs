using Redwood.Framework.Binding;
using System;

namespace Redwood.Framework.Controls
{
    public abstract class GridViewColumn : RedwoodBindableControl 
    {

        [MarkupOptions(AllowBinding = false)]
        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }
        public static readonly RedwoodProperty HeaderTextProperty =
            RedwoodProperty.Register<string, GridViewColumn>(c => c.HeaderText);

        [MarkupOptions(AllowHardCodedValue = false)]
        public object ValueBinding
        {
            get { return GetValue(ValueBindingProperty); }    
            set { SetValue(ValueBindingProperty, value); }
        }
        public static readonly RedwoodProperty ValueBindingProperty =
            RedwoodProperty.Register<object, GridViewColumn>(c => c.ValueBinding);


        [MarkupOptions(AllowBinding = false)]
        public string SortExpression
        {
            get { return (string)GetValue(SortExpressionProperty); }
            set { SetValue(SortExpressionProperty, value); }
        }
        public static readonly RedwoodProperty SortExpressionProperty =
            RedwoodProperty.Register<string, GridViewColumn>(c => c.SortExpression);


        [MarkupOptions(AllowBinding = false)]
        public bool AllowSorting
        {
            get { return (bool)GetValue(AllowSortingProperty); }
            set { SetValue(AllowSortingProperty, value); }
        }
        public static readonly RedwoodProperty AllowSortingProperty =
            RedwoodProperty.Register<bool, GridViewColumn>(c => c.AllowSorting, false);


        [MarkupOptions(AllowBinding = false)]
        public string CssClass
        {
            get { return (string)GetValue(CssClassProperty); }
            set { SetValue(CssClassProperty, value); }
        }
        public static readonly RedwoodProperty CssClassProperty =
            RedwoodProperty.Register<string, GridViewColumn>(c => c.CssClass);


        [MarkupOptions(AllowBinding = false)]
        public string Width
        {
            get { return (string)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }
        public static readonly RedwoodProperty WidthProperty =
            RedwoodProperty.Register<string, GridViewColumn>(c => c.Width);






        public abstract void CreateControls(RedwoodControl container);



        protected ValueBindingExpression CloneValueBinding()
        {
            var binding = GetValueBinding(ValueBindingProperty);
            if (binding == null)
            {
                throw new Exception(string.Format("The ValueBinding property is not set on the {0} control!", GetType()));
            }
            return (ValueBindingExpression)binding.Clone();
        }


        public virtual void CreateHeaderControls(HtmlGenericControl cell)
        {
            // TODO: sorting support
            var literal = new Literal(HeaderText);
            cell.Children.Add(literal);
        }
    }

}