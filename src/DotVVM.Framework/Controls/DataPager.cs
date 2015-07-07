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
    public class DataPager : HtmlGenericControl
    {

        [MarkupOptions(AllowHardCodedValue = false)]
        public IGridViewDataSet DataSet
        {
            get { return (IGridViewDataSet)GetValue(DataSetProperty); }
            set { SetValue(DataSetProperty, value); }
        }
        public static readonly DotvvmProperty DataSetProperty =
            DotvvmProperty.Register<IGridViewDataSet, DataPager>(c => c.DataSet);

        private HtmlGenericControl content;
        private HtmlGenericControl previousLi;
        private HtmlGenericControl nextLi;


        public DataPager() : base("div")
        {
        }

        protected internal override void OnLoad(DotvvmRequestContext context)
        {
            DataBind();
            base.OnLoad(context);
        }

        protected internal override void OnPreRender(DotvvmRequestContext context)
        {
            DataBind();
            base.OnPreRender(context);
        }

        private void DataBind()
        {
            Children.Clear();

            content = new HtmlGenericControl("ul");
            content.SetBinding(DataContextProperty, GetDataSetBinding().Clone());
            Children.Add(content);


            var dataSet = DataSet;
            if (dataSet != null)
            {
                // previous button
                previousLi = new HtmlGenericControl("li");
                var previousLink = new LinkButton() { Text = "«" };
                previousLink.SetBinding(ButtonBase.ClickProperty, new CommandBindingExpression("GoToPreviousPage()"));
                previousLi.Children.Add(previousLink);
                content.Children.Add(previousLi);

                // number fields
                var i = 0;
                foreach (var number in dataSet.NearPageIndexes)
                {
                    var li = new HtmlGenericControl("li");
                    li.SetBinding(DataContextProperty, new ValueBindingExpression("NearPageIndexes[" + i + "]"));
                    if (number == dataSet.PageIndex)
                    {
                        li.Attributes["class"] = "active";
                    }
                    var link = new LinkButton() { Text = (number + 1).ToString() };
                    link.SetBinding(ButtonBase.ClickProperty, new CommandBindingExpression("_parent.GoToPage(_this)"));
                    li.Children.Add(link);
                    content.Children.Add(li);

                    i++;
                }

                // next button
                nextLi = new HtmlGenericControl("li");
                var nextLink = new LinkButton() { Text = "»" };
                nextLink.SetBinding(ButtonBase.ClickProperty, new CommandBindingExpression("GoToNextPage()"));
                nextLi.Children.Add(nextLink);
                content.Children.Add(nextLi);
            }
        }

        protected override void RenderBeginTag(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("with", this, DataSetProperty, () => { });
            writer.AddKnockoutDataBind("visible", "ko.unwrap(" + GetDataSetBinding().TranslateToClientScript(this, DataSetProperty) + ").TotalItemsCount() > 0");
            context.PathFragments.Push(GetDataSetBinding().GetViewModelPathExpression(this, DataSetProperty));
            writer.RenderBeginTag("ul");
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("css", "{ 'disabled': IsFirstPage() }");
            previousLi.Render(writer, context);

            // render template
            writer.WriteKnockoutDataBindComment("foreach", "NearPageIndexes");
            context.PathFragments.Push("NearPageIndexes[$index]");

            writer.AddKnockoutDataBind("css", "{ 'active': $data == $parent.PageIndex()}");
            var li = new HtmlGenericControl("li");
            var link = new LinkButton();
            link.SetBinding(ButtonBase.TextProperty, new ValueBindingExpression("_this + 1"));
            link.SetBinding(ButtonBase.ClickProperty, new CommandBindingExpression("_parent.GoToPage(_this)"));
            li.Children.Add(link);
            li.Render(writer, context);

            context.PathFragments.Pop();
            writer.WriteKnockoutDataBindEndComment();

            writer.AddKnockoutDataBind("css", "{ 'disabled': IsLastPage() }");
            nextLi.Render(writer, context);
        }


        protected override void RenderEndTag(IHtmlWriter writer, RenderContext context)
        {
            writer.RenderEndTag();
            context.PathFragments.Pop();
        }


        private ValueBindingExpression GetDataSetBinding()
        {
            var binding = GetValueBinding(DataSetProperty);
            if (binding == null)
            {
                throw new NotSupportedException("The DataSet property of the rw:DataPager control must be set!");
            }
            return binding;
        }
    }
        
}
