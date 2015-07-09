using Redwood.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    public class DataPager : HtmlGenericControl
    {

        [MarkupOptions(AllowHardCodedValue = false)]
        public IGridViewDataSet DataSet
        {
            get { return (IGridViewDataSet)GetValue(DataSetProperty); }
            set { SetValue(DataSetProperty, value); }
        }
        public static readonly RedwoodProperty DataSetProperty =
            RedwoodProperty.Register<IGridViewDataSet, DataPager>(c => c.DataSet);


        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate FirstPageTemplate
        {
            get { return (ITemplate)GetValue(FirstPageTemplateProperty); }
            set { SetValue(FirstPageTemplateProperty, value); }
        }
        public static readonly RedwoodProperty FirstPageTemplateProperty =
            RedwoodProperty.Register<ITemplate, DataPager>(c => c.FirstPageTemplate, null);

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate LastPageTemplate
        {
            get { return (ITemplate)GetValue(LastPageTemplateProperty); }
            set { SetValue(LastPageTemplateProperty, value); }
        }
        public static readonly RedwoodProperty LastPageTemplateProperty =
            RedwoodProperty.Register<ITemplate, DataPager>(c => c.LastPageTemplate, null);

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate PreviousPageTemplate
        {
            get { return (ITemplate)GetValue(PreviousPageTemplateProperty); }
            set { SetValue(PreviousPageTemplateProperty, value); }
        }
        public static readonly RedwoodProperty PreviousPageTemplateProperty =
            RedwoodProperty.Register<ITemplate, DataPager>(c => c.PreviousPageTemplate, null);

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate NextPageTemplate
        {
            get { return (ITemplate)GetValue(NextPageTemplateProperty); }
            set { SetValue(NextPageTemplateProperty, value); }
        }
        public static readonly RedwoodProperty NextPageTemplateProperty =
            RedwoodProperty.Register<ITemplate, DataPager>(c => c.NextPageTemplate, null);

        [MarkupOptions(AllowBinding = false)]
        public bool RenderLinkForCurrentPage
        {
            get { return (bool)GetValue(RenderLinkForCurrentPageProperty); }
            set { SetValue(RenderLinkForCurrentPageProperty, value); }
        }
        public static readonly RedwoodProperty RenderLinkForCurrentPageProperty =
            RedwoodProperty.Register<bool, DataPager>(c => c.RenderLinkForCurrentPage, null);



        private HtmlGenericControl content;
        private HtmlGenericControl firstLi;
        private HtmlGenericControl previousLi;
        private Placeholder numbersPlaceholder; 
        private HtmlGenericControl nextLi;
        private HtmlGenericControl lastLi;


        public DataPager() : base("div")
        {
        }

        protected internal override void OnLoad(RedwoodRequestContext context)
        {
            DataBind(context);
            base.OnLoad(context);
        }

        protected internal override void OnPreRender(RedwoodRequestContext context)
        {
            DataBind(context);
            base.OnPreRender(context);
        }

        private void DataBind(RedwoodRequestContext context)
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
                firstLink.SetBinding(ButtonBase.ClickProperty, new CommandBindingExpression("GoToFirstPage()"));
                firstLi.Children.Add(firstLink);
                content.Children.Add(firstLi);

                // previous button
                previousLi = new HtmlGenericControl("li");
                var previousLink = new LinkButton();
                SetButtonContent(context, previousLink, "«", PreviousPageTemplate);
                previousLink.SetBinding(ButtonBase.ClickProperty, new CommandBindingExpression("GoToPreviousPage()"));
                previousLi.Children.Add(previousLink);
                content.Children.Add(previousLi);

                // number fields
                numbersPlaceholder = new Placeholder();
                content.Children.Add(numbersPlaceholder);

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
                    numbersPlaceholder.Children.Add(li);

                    i++;
                }

                // next button
                nextLi = new HtmlGenericControl("li");
                var nextLink = new LinkButton();
                SetButtonContent(context, nextLink, "»", NextPageTemplate);
                nextLink.SetBinding(ButtonBase.ClickProperty, new CommandBindingExpression("GoToNextPage()"));
                nextLi.Children.Add(nextLink);
                content.Children.Add(nextLi);

                // last button
                lastLi = new HtmlGenericControl("li");
                var lastLink = new LinkButton();
                SetButtonContent(context, lastLink, "»»", LastPageTemplate);
                lastLink.SetBinding(ButtonBase.ClickProperty, new CommandBindingExpression("GoToLastPage()"));
                lastLi.Children.Add(lastLink);
                content.Children.Add(lastLi);
            }
        }

        private void SetButtonContent(RedwoodRequestContext context, LinkButton button, string text, ITemplate contentTemplate)
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
            firstLi.Render(writer, context);

            writer.AddKnockoutDataBind("css", "{ 'disabled': IsFirstPage() }");
            previousLi.Render(writer, context);

            // render template
            writer.WriteKnockoutDataBindComment("foreach", "NearPageIndexes");
            context.PathFragments.Push("NearPageIndexes[$index]");

            // render page number
            numbersPlaceholder.Children.Clear();
            HtmlGenericControl li;
            if (!RenderLinkForCurrentPage)
            {
                writer.AddKnockoutDataBind("visible", "$data == $parent.PageIndex()");
                li = new HtmlGenericControl("li");
                li.SetValue(Internal.IsDataContextBoundaryProperty, true);
                var literal = new Literal();
                literal.SetBinding(Literal.TextProperty, new ValueBindingExpression("_this + 1"));
                li.Children.Add(literal);
                numbersPlaceholder.Children.Add(li);
                li.Render(writer, context);

                writer.AddKnockoutDataBind("visible", "$data != $parent.PageIndex()");
            }
            writer.AddKnockoutDataBind("css", "{ 'active': $data == $parent.PageIndex()}");
            li = new HtmlGenericControl("li");
            li.SetValue(Internal.IsDataContextBoundaryProperty, true);
            var link = new LinkButton();
            link.SetBinding(ButtonBase.TextProperty, new ValueBindingExpression("_this + 1"));
            link.SetBinding(ButtonBase.ClickProperty, new CommandBindingExpression("_parent.GoToPage(_this)"));
            li.Children.Add(link);
            numbersPlaceholder.Children.Add(li);
            li.Render(writer, context);

            context.PathFragments.Pop();
            writer.WriteKnockoutDataBindEndComment();

            writer.AddKnockoutDataBind("css", "{ 'disabled': IsLastPage() }");
            nextLi.Render(writer, context);

            writer.AddKnockoutDataBind("css", "{ 'disabled': IsLastPage() }");
            lastLi.Render(writer, context);
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
