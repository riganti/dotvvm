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
            new CommandBindingExpression(h => ((IGridViewDataSet)h[0]).GoToNextPage(), "__$DataPager_GoToNextPage");
        private static CommandBindingExpression GoToThisPageCommand =
            new CommandBindingExpression(h => ((IGridViewDataSet)h[1]).GoToPage((int)h[0]), "__$DataPager_GoToThisPage");
        private static CommandBindingExpression GoToPrevPageCommand =
            new CommandBindingExpression(h => ((IGridViewDataSet)h[0]).GoToPreviousPage(), "__$DataPager_GoToPrevPage");
        private static CommandBindingExpression GoToFirstPageCommand =
            new CommandBindingExpression(h => ((IGridViewDataSet)h[0]).GoToFirstPage(), "__$DataPager_GoToFirstPage");
        private static CommandBindingExpression GoToLastPageCommand =
            new CommandBindingExpression(h => ((IGridViewDataSet)h[0]).GoToLastPage(), "__$DataPager_GoToLastPage");


        [MarkupOptions(AllowHardCodedValue = false)]
        public IGridViewDataSet DataSet
        {
            get { return (IGridViewDataSet)GetValue(DataSetProperty); }
            set { SetValue(DataSetProperty, value); }
        }
        public static readonly DotvvmProperty DataSetProperty =
            DotvvmProperty.Register<IGridViewDataSet, DataPager>(c => c.DataSet);


        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate FirstPageTemplate
        {
            get { return (ITemplate)GetValue(FirstPageTemplateProperty); }
            set { SetValue(FirstPageTemplateProperty, value); }
        }
        public static readonly DotvvmProperty FirstPageTemplateProperty =
            DotvvmProperty.Register<ITemplate, DataPager>(c => c.FirstPageTemplate, null);

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate LastPageTemplate
        {
            get { return (ITemplate)GetValue(LastPageTemplateProperty); }
            set { SetValue(LastPageTemplateProperty, value); }
        }
        public static readonly DotvvmProperty LastPageTemplateProperty =
            DotvvmProperty.Register<ITemplate, DataPager>(c => c.LastPageTemplate, null);

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate PreviousPageTemplate
        {
            get { return (ITemplate)GetValue(PreviousPageTemplateProperty); }
            set { SetValue(PreviousPageTemplateProperty, value); }
        }
        public static readonly DotvvmProperty PreviousPageTemplateProperty =
            DotvvmProperty.Register<ITemplate, DataPager>(c => c.PreviousPageTemplate, null);

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate NextPageTemplate
        {
            get { return (ITemplate)GetValue(NextPageTemplateProperty); }
            set { SetValue(NextPageTemplateProperty, value); }
        }
        public static readonly DotvvmProperty NextPageTemplateProperty =
            DotvvmProperty.Register<ITemplate, DataPager>(c => c.NextPageTemplate, null);

        [MarkupOptions(AllowBinding = false)]
        public bool RenderLinkForCurrentPage
        {
            get { return (bool)GetValue(RenderLinkForCurrentPageProperty); }
            set { SetValue(RenderLinkForCurrentPageProperty, value); }
        }
        public static readonly DotvvmProperty RenderLinkForCurrentPageProperty =
            DotvvmProperty.Register<bool, DataPager>(c => c.RenderLinkForCurrentPage, null);



        private HtmlGenericControl content;
        private HtmlGenericControl firstLi;
        private HtmlGenericControl previousLi;
        private Placeholder numbersPlaceholder; 
        private HtmlGenericControl nextLi;
        private HtmlGenericControl lastLi;


        public DataPager() : base("div")
        {
        }

        protected internal override void OnLoad(DotvvmRequestContext context)
        {
            DataBind(context);
            base.OnLoad(context);
        }

        protected internal override void OnPreRender(DotvvmRequestContext context)
        {
            DataBind(context);
            base.OnPreRender(context);
        }

        private void DataBind(DotvvmRequestContext context)
        {
            Children.Clear();

            content = new HtmlGenericControl("ul");
            content.SetBinding(DataContextProperty, GetDataSetBinding().Clone());
            Children.Add(content);


            var dataSet = DataSet;
            if (dataSet != null)
            {
                // first button
                firstLi = new HtmlGenericControl("li");
                var firstLink = new LinkButton();
                SetButtonContent(context, firstLink, "««", FirstPageTemplate);
                firstLink.SetBinding(ButtonBase.ClickProperty, GoToFirstPageCommand);
                firstLi.Children.Add(firstLink);
                content.Children.Add(firstLi);

                // previous button
                previousLi = new HtmlGenericControl("li");
                var previousLink = new LinkButton();
                SetButtonContent(context, previousLink, "«", PreviousPageTemplate);
                previousLink.SetBinding(ButtonBase.ClickProperty, GoToPrevPageCommand);
                previousLi.Children.Add(previousLink);
                content.Children.Add(previousLi);

                // number fields
                numbersPlaceholder = new Placeholder();
                content.Children.Add(numbersPlaceholder);

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
                    numbersPlaceholder.Children.Add(li);

                    i++;
                }

                // next button
                nextLi = new HtmlGenericControl("li");
                var nextLink = new LinkButton();
                SetButtonContent(context, nextLink, "»", NextPageTemplate);
                nextLink.SetBinding(ButtonBase.ClickProperty, GoToNextPageCommand);
                nextLi.Children.Add(nextLink);
                content.Children.Add(nextLi);

                // last button
                lastLi = new HtmlGenericControl("li");
                var lastLink = new LinkButton();
                SetButtonContent(context, lastLink, "»»", LastPageTemplate);
                lastLink.SetBinding(ButtonBase.ClickProperty, GoToLastPageCommand);
                lastLi.Children.Add(lastLink);
                content.Children.Add(lastLi);
            }
        }

        private void SetButtonContent(DotvvmRequestContext context, LinkButton button, string text, ITemplate contentTemplate)
        {
            if (contentTemplate != null)
            {
                contentTemplate.BuildContent(context, button);
            }
            else
            {
                button.Text = text;
            }
        }

        private ValueBindingExpression GetNearIndexesBinding(int i)
        {
            return new ValueBindingExpression(
                        (h, c) => ((IGridViewDataSet)h[0]).NearPageIndexes[i],
                        "NearPageIndexes[" + i + "]");
        }

        protected override void RenderBeginTag(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("with", this, DataSetProperty, () => { });
            writer.AddKnockoutDataBind("visible", "ko.unwrap(" + GetDataSetBinding().TranslateToClientScript(this, DataSetProperty) + ").TotalItemsCount() > 0");
            writer.RenderBeginTag("ul");
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("css", "{ 'disabled': IsFirstPage() }");
            firstLi.Render(writer, context);

            writer.AddKnockoutDataBind("css", "{ 'disabled': IsFirstPage() }");
            previousLi.Render(writer, context);

            // render template
            writer.WriteKnockoutForeachComment("NearPageIndexes");

            // render page number
            numbersPlaceholder.Children.Clear();
            HtmlGenericControl li;
            if (!RenderLinkForCurrentPage)
            {
                writer.AddKnockoutDataBind("visible", "$data == $parent.PageIndex()");
                li = new HtmlGenericControl("li");
                var literal = new Literal();
                literal.SetBinding(Literal.TextProperty, new ValueBindingExpression(vm => (int)vm[0] + 1, "$data + 1"));
                li.Children.Add(literal);
                numbersPlaceholder.Children.Add(li);
                li.Render(writer, context);

                writer.AddKnockoutDataBind("visible", "$data != $parent.PageIndex()");
            }
            writer.AddKnockoutDataBind("css", "{ 'active': $data == $parent.PageIndex()}");
            li = new HtmlGenericControl("li");
            li.SetValue(Internal.PathFragmentProperty, "NearPageIndexes[$index]");
            var link = new LinkButton();
            li.Children.Add(link);
            link.SetBinding(ButtonBase.TextProperty, new ValueBindingExpression(vm => (int)vm[0] + 1, "$data + 1"));
            link.SetBinding(ButtonBase.ClickProperty, GoToThisPageCommand);
            numbersPlaceholder.Children.Add(li);
            li.Render(writer, context);

            writer.WriteKnockoutDataBindEndComment();

            writer.AddKnockoutDataBind("css", "{ 'disabled': IsLastPage() }");
            nextLi.Render(writer, context);

            writer.AddKnockoutDataBind("css", "{ 'disabled': IsLastPage() }");
            lastLi.Render(writer, context);
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
                throw new NotSupportedException("The DataSet property of the dot:DataPager control must be set!");
            }
            return binding;
        }
    }

}
