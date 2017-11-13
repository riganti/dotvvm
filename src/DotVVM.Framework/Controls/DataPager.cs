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
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the pagination control which can be integrated with the GridViewDataSet object to provide the paging capabilities.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class DataPager : HtmlGenericControl
    {
        public class CommonBindings
        {
            public readonly CommandBindingExpression GoToNextPageCommand;
            public readonly CommandBindingExpression GoToThisPageCommand;
            public readonly CommandBindingExpression GoToPrevPageCommand;
            public readonly CommandBindingExpression GoToFirstPageCommand;
            public readonly CommandBindingExpression GoToLastPageCommand;

            public CommonBindings(BindingCompilationService service)
            {
                GoToNextPageCommand = new CommandBindingExpression(service, h => ((IPageableGridViewDataSet)h[0]).GoToNextPageAsync(), "__$DataPager_GoToNextPage");
                GoToThisPageCommand = new CommandBindingExpression(service, h => ((IPageableGridViewDataSet)h[1]).GoToPageAsync((int)h[0]), "__$DataPager_GoToThisPage");
                GoToPrevPageCommand = new CommandBindingExpression(service, h => ((IPageableGridViewDataSet)h[0]).GoToPreviousPageAsync(), "__$DataPager_GoToPrevPage");
                GoToFirstPageCommand = new CommandBindingExpression(service, h => ((IPageableGridViewDataSet)h[0]).GoToFirstPageAsync(), "__$DataPager_GoToFirstPage");
                GoToLastPageCommand = new CommandBindingExpression(service, h => ((IPageableGridViewDataSet)h[0]).GoToLastPageAsync(), "__$DataPager_GoToLastPage");
            }
        }
        private readonly CommonBindings commonBindings;
        private readonly BindingCompilationService bindingService;

        public DataPager(CommonBindings commonBindings, BindingCompilationService bindingService)
            : base("div")
        {
            this.commonBindings = commonBindings;
            this.bindingService = bindingService;
        }


        /// <summary>
        /// Gets or sets the GridViewDataSet object in the viewmodel.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public IPageableGridViewDataSet DataSet
        {
            get { return (IPageableGridViewDataSet)GetValue(DataSetProperty); }
            set { SetValue(DataSetProperty, value); }
        }
        public static readonly DotvvmProperty DataSetProperty =
            DotvvmProperty.Register<IPageableGridViewDataSet, DataPager>(c => c.DataSet);


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
        public static readonly DotvvmProperty EnabledProperty =
            DotvvmPropertyWithFallback.Register<bool, DataPager>(nameof(Enabled), FormControls.EnabledProperty);


        private HtmlGenericControl content;
        private HtmlGenericControl firstLi;
        private HtmlGenericControl previousLi;
        private PlaceHolder numbersPlaceHolder;
        private HtmlGenericControl nextLi;
        private HtmlGenericControl lastLi;
        
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
        private void CallGridViewDataSetRefreshRequest(IRefreshableGridViewDataSet gridViewDataSet)
        {
            gridViewDataSet.RequestRefresh();
        }

        private void DataBind(Hosting.IDotvvmRequestContext context)
        {
            if (DataSet is IRefreshableGridViewDataSet refreshableDataSet)
            {
                CallGridViewDataSetRefreshRequest(refreshableDataSet);
            }

            Children.Clear();

            content = new HtmlGenericControl("ul");
            var dataContextType = DataContextStack.Create(typeof(IPageableGridViewDataSet), this.GetDataContextType());
            content.SetDataContextType(dataContextType);
            content.SetBinding(DataContextProperty, GetDataSetBinding());
            Children.Add(content);

            var bindings = context.Services.GetService<CommonBindings>();

            var dataSet = DataSet;
            if (dataSet != null)
            {
                object enabledValue = HasValueBinding(EnabledProperty) ?
                    (object)ValueBindingExpression.CreateBinding<bool>(
                        bindingService.WithoutInitialization(),
                        h => (bool)GetValueBinding(EnabledProperty).Evaluate(this),
                        new JsSymbolicParameter(JavascriptTranslator.KnockoutContextParameter).Member("$pagerEnabled")) :
                    Enabled;

                // first button
                firstLi = new HtmlGenericControl("li");
                var firstLink = new LinkButton();
                SetButtonContent(context, firstLink, "««", FirstPageTemplate);
                firstLink.SetBinding(ButtonBase.ClickProperty, bindings.GoToFirstPageCommand);
                if (!true.Equals(enabledValue)) firstLink.SetValue(LinkButton.EnabledProperty, enabledValue);
                firstLi.Children.Add(firstLink);
                content.Children.Add(firstLi);

                // previous button
                previousLi = new HtmlGenericControl("li");
                var previousLink = new LinkButton();
                SetButtonContent(context, previousLink, "«", PreviousPageTemplate);
                previousLink.SetBinding(ButtonBase.ClickProperty, bindings.GoToPrevPageCommand);
                if (!true.Equals(enabledValue)) previousLink.SetValue(LinkButton.EnabledProperty, enabledValue);
                previousLi.Children.Add(previousLink);
                content.Children.Add(previousLi);

                // number fields
                numbersPlaceHolder = new PlaceHolder();
                content.Children.Add(numbersPlaceHolder);

                var i = 0;
                foreach (var number in dataSet.PagingOptions.NearPageIndexes)
                {
                    var li = new HtmlGenericControl("li");
                    li.SetBinding(DataContextProperty, GetNearIndexesBinding(context, i, dataContextType));
                    if (number == dataSet.PagingOptions.PageIndex)
                    {
                        li.Attributes["class"] = "active";
                    }
                    var link = new LinkButton() { Text = (number + 1).ToString() };
                    link.SetBinding(ButtonBase.ClickProperty, bindings.GoToThisPageCommand);
                    if (!true.Equals(enabledValue)) link.SetValue(LinkButton.EnabledProperty, enabledValue);
                    li.Children.Add(link);
                    numbersPlaceHolder.Children.Add(li);

                    i++;
                }

                // next button
                nextLi = new HtmlGenericControl("li");
                var nextLink = new LinkButton();
                SetButtonContent(context, nextLink, "»", NextPageTemplate);
                nextLink.SetBinding(ButtonBase.ClickProperty, bindings.GoToNextPageCommand);
                if (!true.Equals(enabledValue)) nextLink.SetValue(LinkButton.EnabledProperty, enabledValue);
                nextLi.Children.Add(nextLink);
                content.Children.Add(nextLi);

                // last button
                lastLi = new HtmlGenericControl("li");
                var lastLink = new LinkButton();
                SetButtonContent(context, lastLink, "»»", LastPageTemplate);
                if (!true.Equals(enabledValue)) lastLink.SetValue(LinkButton.EnabledProperty, enabledValue);
                lastLink.SetBinding(ButtonBase.ClickProperty, bindings.GoToLastPageCommand);
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

        private ConditionalWeakTable<DotvvmConfiguration, ConcurrentDictionary<int, ValueBindingExpression>> _nearIndexesBindingCache = new ConditionalWeakTable<DotvvmConfiguration, ConcurrentDictionary<int, ValueBindingExpression>>();
        private ValueBindingExpression GetNearIndexesBinding(IDotvvmRequestContext context, int i, DataContextStack dataContext = null)
        {
            return
                _nearIndexesBindingCache.GetOrCreateValue(context.Configuration)
                .GetOrAdd(i, _ =>
                ValueBindingExpression.CreateBinding(
                bindingService.WithoutInitialization(),
                h => ((IPageableGridViewDataSet)h[0]).PagingOptions.NearPageIndexes[i], dataContext));
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
                    writer.AddKnockoutDataBind("visible", $"ko.unwrap({GetDataSetBinding().GetKnockoutBindingExpression(this)}).PagingOptions().PagesCount() > 1");
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
                writer.WriteKnockoutDataBindComment("dotvvm_introduceAlias",
                    $"{{ '$pagerEnabled': { GetValueBinding(EnabledProperty).GetKnockoutBindingExpression(this) }}}");

            if (HasBinding(EnabledProperty))
            {
                writer.AddKnockoutDataBind("css", "{disabled:$pagerEnabled()}");
            }
            else
            {
                writer.AddKnockoutDataBind("css", $"{{ 'disabled': { (!Enabled).ToString().ToLower() } }}");
            }

            writer.AddKnockoutDataBind("with", this, DataSetProperty, renderEvenInServerRenderingMode: true);
            writer.RenderBeginTag("ul");

        }

        private static ParametrizedCode currentPageTextJs = new JsBinaryExpression(new JsBinaryExpression(new JsLiteral(1), BinaryOperatorType.Plus, new JsSymbolicParameter(JavascriptTranslator.KnockoutViewModelParameter)), BinaryOperatorType.Plus, new JsLiteral("")).FormatParametrizedScript();

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {

            writer.AddKnockoutDataBind("css", "{ 'disabled': PagingOptions().IsFirstPage() }");
            firstLi.Render(writer, context);

            writer.AddKnockoutDataBind("css", "{ 'disabled': PagingOptions().IsFirstPage() }");
            previousLi.Render(writer, context);

            // render template
            writer.WriteKnockoutForeachComment("PagingOptions().NearPageIndexes");

            // render page number
            numbersPlaceHolder.Children.Clear();
            HtmlGenericControl li;
            var currentPageTextContext = DataContextStack.Create(typeof(int), numbersPlaceHolder.GetDataContextType());
            var currentPageTextBinding = ValueBindingExpression.CreateBinding(bindingService.WithoutInitialization(),
                    vm => ((int)vm[0] + 1).ToString(),
                    currentPageTextJs,
                    currentPageTextContext);
            if (!RenderLinkForCurrentPage)
            {
                writer.AddKnockoutDataBind("visible", "$data == $parent.PagingOptions().PageIndex()");
                writer.AddKnockoutDataBind("css", "{'active': $data == $parent.PagingOptions().PageIndex()}");
                li = new HtmlGenericControl("li");
                var literal = new Literal();
                literal.DataContext = 0;
                literal.SetDataContextType(currentPageTextContext);

                literal.SetBinding(Literal.TextProperty, currentPageTextBinding);
                li.Children.Add(literal);
                numbersPlaceHolder.Children.Add(li);
                li.Render(writer, context);

                writer.AddKnockoutDataBind("visible", "$data != $parent.PagingOptions().PageIndex()");
            }
            writer.AddKnockoutDataBind("css", "{ 'active': $data == $parent.PagingOptions().PageIndex()}");
            li = new HtmlGenericControl("li");
            li.SetValue(Internal.PathFragmentProperty, "PagingOptions.NearPageIndexes[$index]");
            var link = new LinkButton();
            li.Children.Add(link);
            link.SetDataContextType(currentPageTextContext);
            link.SetBinding(ButtonBase.TextProperty, currentPageTextBinding);
            link.SetBinding(ButtonBase.ClickProperty, commonBindings.GoToThisPageCommand);
            object enabledValue = HasValueBinding(EnabledProperty) ?
                (object)ValueBindingExpression.CreateBinding(bindingService.WithoutInitialization(),
                    h => GetValueBinding(EnabledProperty).Evaluate(this),
                    new JsSymbolicParameter(JavascriptTranslator.KnockoutContextParameter).Member("$pagerEnabled")) :
                Enabled;
            if (!true.Equals(enabledValue)) link.SetValue(LinkButton.EnabledProperty, enabledValue);
            numbersPlaceHolder.Children.Add(li);
            li.Render(writer, context);

            writer.WriteKnockoutDataBindEndComment();

            writer.AddKnockoutDataBind("css", "{ 'disabled': PagingOptions().IsLastPage() }");
            nextLi.Render(writer, context);

            writer.AddKnockoutDataBind("css", "{ 'disabled': PagingOptions().IsLastPage() }");
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
