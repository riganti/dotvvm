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
        private static CommandBindingExpression GoToNextPageCommand =
            new CommandBindingExpression(h => ((IGridViewDataSet)h[h.Length - 1]).GoToNextPage(), "__$DataPager_GoToNextPage");
        private static CommandBindingExpression GoToThisPageCommand =
            new CommandBindingExpression(h => ((IGridViewDataSet)h[h.Length - 2]).GoToPage((int)h[h.Length - 1]), "__$DataPager_GoToThisPage");
        private static CommandBindingExpression GoToPrevPageCommand =
            new CommandBindingExpression(h => ((IGridViewDataSet)h[h.Length - 1]).GoToPreviousPage(), "__$DataPager_GoToPrevPage");


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
                previousLink.SetBinding(ButtonBase.ClickProperty, GoToPrevPageCommand);
                previousLi.Children.Add(previousLink);
                content.Children.Add(previousLi);

                // number fields
                var i = 0;
                foreach (var number in dataSet.NearPageIndexes)
                {
                    var li = new HtmlGenericControl("li");
                    li.SetBinding(DataContextProperty, GetNearIndexesBinding(i));
                    if (number == dataSet.PageIndex)
                    {
                        li.Attributes["class"] = "active";
                    }
                    var link = new LinkButton() { Text = (number + 1).ToString() };
                    link.SetBinding(ButtonBase.ClickProperty, GoToThisPageCommand);
                    li.Children.Add(link);
                    content.Children.Add(li);

                    i++;
                }

                // next button
                nextLi = new HtmlGenericControl("li");
                var nextLink = new LinkButton() { Text = "»" };
                nextLink.SetBinding(ButtonBase.ClickProperty, GoToNextPageCommand);
                nextLi.Children.Add(nextLink);
                content.Children.Add(nextLi);
            }
        }

        private ValueBindingExpression GetNearIndexesBinding(int i)
        {
            return new ValueBindingExpression(
                        (h, c) => ((IGridViewDataSet)h[h.Length - 1]).NearPageIndexes[i],
                        "NearPageIndexes[" + i + "]");
        }

        protected override void RenderBeginTag(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("with", this, DataSetProperty, () => { });
            writer.AddAttribute("data-path", GetBinding(DataSetProperty).Javascript);
            writer.AddKnockoutDataBind("visible", "ko.unwrap(" + GetDataSetBinding().TranslateToClientScript(this, DataSetProperty) + ").TotalItemsCount() > 0");
            writer.RenderBeginTag("ul");
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("css", "{ 'disabled': IsFirstPage() }");
            previousLi.Render(writer, context);

            // render template
            writer.WriteKnockoutDataBindComment("foreach", "NearPageIndexes");

            writer.AddKnockoutDataBind("css", "{ 'active': $data == $parent.PageIndex()}");
            var li = new HtmlGenericControl("li");
            li.Attributes["data-path"] = "NearPageIndexes[$index]";
            var link = new LinkButton();
            li.Children.Add(link);
            link.SetBinding(ButtonBase.TextProperty, new ValueBindingExpression(vm => (int)vm.Last() + 1, "$data + 1"));
            //link.SetBinding(ButtonBase.ClickProperty, new CommandBindingExpression("_parent.GoToPage(_this)"));
            link.SetBinding(ButtonBase.ClickProperty, GoToThisPageCommand);
            li.Render(writer, context);

            writer.WriteKnockoutDataBindEndComment();

            writer.AddKnockoutDataBind("css", "{ 'disabled': IsLastPage() }");
            nextLi.Render(writer, context);
        }


        protected override void RenderEndTag(IHtmlWriter writer, RenderContext context)
        {
            writer.RenderEndTag();
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
