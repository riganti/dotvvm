using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Compilation.Javascript;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the pagination control which can be integrated with the GridViewDataSet object to provide the paging capabilities.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class DataPager : HtmlGenericControl
    {
        // t+
        private static CommandBindingExpression GoToNextPageCommand =
            new CommandBindingExpression(new BindingCompilationService(), h => ((IGridViewDataSet)h[0]).GoToNextPage(), "__$DataPager_GoToNextPage");
        private static CommandBindingExpression GoToThisPageCommand =
            new CommandBindingExpression(new BindingCompilationService(), h => ((IGridViewDataSet)h[1]).GoToPage((int)h[0]), "__$DataPager_GoToThisPage");
        private static CommandBindingExpression GoToPrevPageCommand =
            new CommandBindingExpression(new BindingCompilationService(), h => ((IGridViewDataSet)h[0]).GoToPreviousPage(), "__$DataPager_GoToPrevPage");
        private static CommandBindingExpression GoToFirstPageCommand =
            new CommandBindingExpression(new BindingCompilationService(), h => ((IGridViewDataSet)h[0]).GoToFirstPage(), "__$DataPager_GoToFirstPage");
        private static CommandBindingExpression GoToLastPageCommand =
            new CommandBindingExpression(new BindingCompilationService(), h => ((IGridViewDataSet)h[0]).GoToLastPage(), "__$DataPager_GoToLastPage");


        /// <summary>
        /// Gets or sets the GridViewDataSet object in the viewmodel.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public IGridViewDataSet DataSet
        {
            get { return (IGridViewDataSet)GetValue(DataSetProperty); }
            set { SetValue(DataSetProperty, value); }
        }
        public static readonly DotvvmProperty DataSetProperty =
            DotvvmProperty.Register<IGridViewDataSet, DataPager>(c => c.DataSet);


        /// <summary>
        /// Gets or sets the template of the button which moves the user to the first page.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate FirstPageTemplate
        {
            get { return (ITemplate)GetValue(FirstPageTemplateProperty); }
            set { SetValue(FirstPageTemplateProperty, value); }
        }
        public static readonly DotvvmProperty FirstPageTemplateProperty =
            DotvvmProperty.Register<ITemplate, DataPager>(c => c.FirstPageTemplate, null);

        /// <summary>
        /// Gets or sets the template of the button which moves the user to the last page.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate LastPageTemplate
        {
            get { return (ITemplate)GetValue(LastPageTemplateProperty); }
            set { SetValue(LastPageTemplateProperty, value); }
        }
        public static readonly DotvvmProperty LastPageTemplateProperty =
            DotvvmProperty.Register<ITemplate, DataPager>(c => c.LastPageTemplate, null);

        /// <summary>
        /// Gets or sets the template of the button which moves the user to the previous page.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate PreviousPageTemplate
        {
            get { return (ITemplate)GetValue(PreviousPageTemplateProperty); }
            set { SetValue(PreviousPageTemplateProperty, value); }
        }
        public static readonly DotvvmProperty PreviousPageTemplateProperty =
            DotvvmProperty.Register<ITemplate, DataPager>(c => c.PreviousPageTemplate, null);

        /// <summary>
        /// Gets or sets the template of the button which moves the user to the next page.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate NextPageTemplate
        {
            get { return (ITemplate)GetValue(NextPageTemplateProperty); }
            set { SetValue(NextPageTemplateProperty, value); }
        }
        public static readonly DotvvmProperty NextPageTemplateProperty =
            DotvvmProperty.Register<ITemplate, DataPager>(c => c.NextPageTemplate, null);

        /// <summary>
        /// Gets or sets whether a hyperlink should be rendered for the current page number. If set to false, only a plain text is rendered.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool RenderLinkForCurrentPage
        {
            get { return (bool)GetValue(RenderLinkForCurrentPageProperty); }
            set { SetValue(RenderLinkForCurrentPageProperty, value); }
        }
        public static readonly DotvvmProperty RenderLinkForCurrentPageProperty =
            DotvvmProperty.Register<bool, DataPager>(c => c.RenderLinkForCurrentPage);


        /// <summary>
        /// Gets or sets whether the pager should hide automatically when there is only one page of results. Must not be set to true when using the Visible property.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool HideWhenOnlyOnePage
        {
            get { return (bool)GetValue(HideWhenOnlyOnePageProperty); }
            set { SetValue(HideWhenOnlyOnePageProperty, value); }
        }
        public static readonly DotvvmProperty HideWhenOnlyOnePageProperty
            = DotvvmProperty.Register<bool, DataPager>(c => c.HideWhenOnlyOnePage, true);

        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty
            = DotvvmProperty.Register<bool, DataPager>(c => c.Enabled, true);



        private HtmlGenericControl content;
        private HtmlGenericControl firstLi;
        private HtmlGenericControl previousLi;
        private PlaceHolder numbersPlaceHolder;
        private HtmlGenericControl nextLi;
        private HtmlGenericControl lastLi;


        public DataPager() : base("div")
        {
        }

        protected internal override void OnLoad(Hosting.IDotvvmRequestContext context)
        {
            DataBind(context);
            base.OnLoad(context);
        }

        protected internal override void OnPreRender(Hosting.IDotvvmRequestContext context)
        {
            DataBind(context);
            base.OnPreRender(context);
        }

        private void DataBind(Hosting.IDotvvmRequestContext context)
        {
            Children.Clear();

            content = new HtmlGenericControl("ul");
            content.SetBinding(DataContextProperty, GetDataSetBinding());
            Children.Add(content);


            var dataSet = DataSet;
            if (dataSet != null)
            {
                object enabledValue = HasValueBinding(EnabledProperty) ? 
                    (object)ValueBindingExpression.CreateBinding<bool>(
                        context.Configuration.ServiceLocator.GetService<BindingCompilationService>(),
                        h => (bool)GetValueBinding(EnabledProperty).Evaluate(this),
                        new JsSymbolicParameter(JavascriptTranslator.KnockoutContextParameter).Member("$pagerEnabled")) :
                    Enabled;

                // first button
                firstLi = new HtmlGenericControl("li");
                var firstLink = new LinkButton();
                SetButtonContent(context, firstLink, "««", FirstPageTemplate);
                firstLink.SetBinding(ButtonBase.ClickProperty, GoToFirstPageCommand);
                if (!true.Equals(enabledValue)) firstLink.SetValue(LinkButton.EnabledProperty, enabledValue);
                firstLi.Children.Add(firstLink);
                content.Children.Add(firstLi);

                // previous button
                previousLi = new HtmlGenericControl("li");
                var previousLink = new LinkButton();
                SetButtonContent(context, previousLink, "«", PreviousPageTemplate);
                previousLink.SetBinding(ButtonBase.ClickProperty, GoToPrevPageCommand);
                if (!true.Equals(enabledValue)) previousLink.SetValue(LinkButton.EnabledProperty, enabledValue);
                previousLi.Children.Add(previousLink);
                content.Children.Add(previousLi);

                // number fields
                numbersPlaceHolder = new PlaceHolder();
                content.Children.Add(numbersPlaceHolder);

                var i = 0;
                foreach (var number in dataSet.NearPageIndexes)
                {
                    var li = new HtmlGenericControl("li");
                    li.SetBinding(DataContextProperty, GetNearIndexesBinding(context, i));
                    if (number == dataSet.PageIndex)
                    {
                        li.Attributes["class"] = "active";
                    }
                    var link = new LinkButton() { Text = (number + 1).ToString() };
                    link.SetBinding(ButtonBase.ClickProperty, GoToThisPageCommand);
                    if (!true.Equals(enabledValue)) link.SetValue(LinkButton.EnabledProperty, enabledValue);
                    li.Children.Add(link);
                    numbersPlaceHolder.Children.Add(li);

                    i++;
                }

                // next button
                nextLi = new HtmlGenericControl("li");
                var nextLink = new LinkButton();
                SetButtonContent(context, nextLink, "»", NextPageTemplate);
                nextLink.SetBinding(ButtonBase.ClickProperty, GoToNextPageCommand);
                if (!true.Equals(enabledValue)) nextLink.SetValue(LinkButton.EnabledProperty, enabledValue);
                nextLi.Children.Add(nextLink);
                content.Children.Add(nextLi);

                // last button
                lastLi = new HtmlGenericControl("li");
                var lastLink = new LinkButton();
                SetButtonContent(context, lastLink, "»»", LastPageTemplate);
                if (!true.Equals(enabledValue)) lastLink.SetValue(LinkButton.EnabledProperty, enabledValue);
                lastLink.SetBinding(ButtonBase.ClickProperty, GoToLastPageCommand);
                lastLi.Children.Add(lastLink);
                content.Children.Add(lastLi);
            }
        }

        private void SetButtonContent(Hosting.IDotvvmRequestContext context, LinkButton button, string text, ITemplate contentTemplate)
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

        private ValueBindingExpression GetNearIndexesBinding(IDotvvmRequestContext context, int i)
        {
            return ValueBindingExpression.CreateBinding(
                context.Configuration.ServiceLocator.GetService<BindingCompilationService>(),
                h => ((IGridViewDataSet)h[0]).NearPageIndexes[i]);
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderOnServer)
            {
                throw new DotvvmControlException(this, "The DataPager control cannot be rendered in the RenderSettings.Mode='Server'.");
            }

            base.AddAttributesToRender(writer, context);
        }

        protected override void AddVisibleAttributeOrBinding(IHtmlWriter writer)
        {
            if (!IsPropertySet(VisibleProperty))
            {
                if (HideWhenOnlyOnePage)
                {
                    writer.AddKnockoutDataBind("visible", $"ko.unwrap({GetDataSetBinding().GetKnockoutBindingExpression()}).PagesCount() > 1");
                }
                else
                {
                    writer.AddKnockoutDataBind("visible", this, VisibleProperty, renderEvenInServerRenderingMode: true);
                }
            }
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (HasValueBinding(EnabledProperty))
                writer.WriteKnockoutDataBindComment("dotvvm_introduceAlias", $"{{ '$pagerEnabled': { GetValueBinding(EnabledProperty).GetKnockoutBindingExpression() }}}");

            writer.AddKnockoutDataBind("with", this, DataSetProperty, renderEvenInServerRenderingMode: true);
            writer.RenderBeginTag("ul");
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddKnockoutDataBind("css", "{ 'disabled': IsFirstPage() }");
            firstLi.Render(writer, context);

            writer.AddKnockoutDataBind("css", "{ 'disabled': IsFirstPage() }");
            previousLi.Render(writer, context);

            // render template
            writer.WriteKnockoutForeachComment("NearPageIndexes");

            // render page number
            numbersPlaceHolder.Children.Clear();
            HtmlGenericControl li;
            if (!RenderLinkForCurrentPage)
            {
                writer.AddKnockoutDataBind("visible", "$data == $parent.PageIndex()");
                writer.AddKnockoutDataBind("css", "{'active': $data == $parent.PageIndex()}");
                li = new HtmlGenericControl("li");
                var literal = new Literal();
                literal.DataContext = 0;

                literal.SetBinding(Literal.TextProperty, ValueBindingExpression.CreateBinding(context.Configuration.ServiceLocator.GetService<BindingCompilationService>(),
                    vm => ((int)vm[0] + 1).ToString()));
                li.Children.Add(literal);
                numbersPlaceHolder.Children.Add(li);
                li.Render(writer, context);

                writer.AddKnockoutDataBind("visible", "$data != $parent.PageIndex()");
            }
            writer.AddKnockoutDataBind("css", "{ 'active': $data == $parent.PageIndex()}");
            li = new HtmlGenericControl("li");
            li.SetValue(Internal.PathFragmentProperty, "NearPageIndexes[$index]");
            var link = new LinkButton();
            li.Children.Add(link);
            link.SetBinding(ButtonBase.TextProperty, ValueBindingExpression.CreateBinding(context.Configuration.ServiceLocator.GetService<BindingCompilationService>(),
                vm => ((int)vm[0] + 1).ToString()));
            link.SetBinding(ButtonBase.ClickProperty, GoToThisPageCommand);
            object enabledValue = HasValueBinding(EnabledProperty) ?
                (object)ValueBindingExpression.CreateBinding(context.Configuration.ServiceLocator.GetService<BindingCompilationService>(), 
                    h => GetValueBinding(EnabledProperty).Evaluate(this),
                    new JsSymbolicParameter(JavascriptTranslator.KnockoutContextParameter).Member("$pagerEnabled")) :
                Enabled;
            if (!true.Equals(enabledValue)) link.SetValue(LinkButton.EnabledProperty, enabledValue);
            numbersPlaceHolder.Children.Add(li);
            li.Render(writer, context);

            writer.WriteKnockoutDataBindEndComment();

            writer.AddKnockoutDataBind("css", "{ 'disabled': IsLastPage() }");
            nextLi.Render(writer, context);

            writer.AddKnockoutDataBind("css", "{ 'disabled': IsLastPage() }");
            lastLi.Render(writer, context);
        }


        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.RenderEndTag();
            if (HasValueBinding(EnabledProperty)) writer.WriteKnockoutDataBindEndComment();
        }

        private IValueBinding GetDataSetBinding()
        {
            var binding = GetValueBinding(DataSetProperty);
            if (binding == null)
            {
                throw new DotvvmControlException(this, "The DataSet property of the dot:DataPager control must be set!");
            }
            return binding;
        }
    }

}
