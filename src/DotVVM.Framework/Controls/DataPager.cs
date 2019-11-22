#nullable enable
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using DotVVM.Framework.Utils;

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
                GoToNextPageCommand = new CommandBindingExpression(service, h => ((IPageableGridViewDataSet)h[0]).GoToNextPage(), "__$DataPager_GoToNextPage");
                GoToThisPageCommand = new CommandBindingExpression(service, h => ((IPageableGridViewDataSet)h[1]).GoToPage((int)h[0]), "__$DataPager_GoToThisPage");
                GoToPrevPageCommand = new CommandBindingExpression(service, h => ((IPageableGridViewDataSet)h[0]).GoToPreviousPage(), "__$DataPager_GoToPrevPage");
                GoToFirstPageCommand = new CommandBindingExpression(service, h => ((IPageableGridViewDataSet)h[0]).GoToFirstPage(), "__$DataPager_GoToFirstPage");
                GoToLastPageCommand = new CommandBindingExpression(service, h => ((IPageableGridViewDataSet)h[0]).GoToLastPage(), "__$DataPager_GoToLastPage");
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
        public IPageableGridViewDataSet? DataSet
        {
            get { return (IPageableGridViewDataSet?)GetValue(DataSetProperty); }
            set { SetValue(DataSetProperty, value); }
        }
        public static readonly DotvvmProperty DataSetProperty =
            DotvvmProperty.Register<IPageableGridViewDataSet?, DataPager>(c => c.DataSet);

        /// <summary>
        /// Gets or sets the template of the button which moves the user to the first page.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate? FirstPageTemplate
        {
            get { return (ITemplate?)GetValue(FirstPageTemplateProperty); }
            set { SetValue(FirstPageTemplateProperty, value); }
        }
        public static readonly DotvvmProperty FirstPageTemplateProperty =
            DotvvmProperty.Register<ITemplate?, DataPager>(c => c.FirstPageTemplate, null);

        /// <summary>
        /// Gets or sets the template of the button which moves the user to the last page.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate? LastPageTemplate
        {
            get { return (ITemplate?)GetValue(LastPageTemplateProperty); }
            set { SetValue(LastPageTemplateProperty, value); }
        }
        public static readonly DotvvmProperty LastPageTemplateProperty =
            DotvvmProperty.Register<ITemplate?, DataPager>(c => c.LastPageTemplate, null);

        /// <summary>
        /// Gets or sets the template of the button which moves the user to the previous page.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate? PreviousPageTemplate
        {
            get { return (ITemplate?)GetValue(PreviousPageTemplateProperty); }
            set { SetValue(PreviousPageTemplateProperty, value); }
        }
        public static readonly DotvvmProperty PreviousPageTemplateProperty =
            DotvvmProperty.Register<ITemplate?, DataPager>(c => c.PreviousPageTemplate, null);

        /// <summary>
        /// Gets or sets the template of the button which moves the user to the next page.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate? NextPageTemplate
        {
            get { return (ITemplate?)GetValue(NextPageTemplateProperty); }
            set { SetValue(NextPageTemplateProperty, value); }
        }
        public static readonly DotvvmProperty NextPageTemplateProperty =
            DotvvmProperty.Register<ITemplate?, DataPager>(c => c.NextPageTemplate, null);

        /// <summary>
        /// Gets or sets whether a hyperlink should be rendered for the current page number. If set to false, only a plain text is rendered.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool RenderLinkForCurrentPage
        {
            get { return (bool)GetValue(RenderLinkForCurrentPageProperty)!; }
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
            get { return (bool)GetValue(HideWhenOnlyOnePageProperty)!; }
            set { SetValue(HideWhenOnlyOnePageProperty, value); }
        }
        public static readonly DotvvmProperty HideWhenOnlyOnePageProperty
            = DotvvmProperty.Register<bool, DataPager>(c => c.HideWhenOnlyOnePage, true);

        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty)!; }
            set { SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty =
            DotvvmPropertyWithFallback.Register<bool, DataPager>(nameof(Enabled), FormControls.EnabledProperty);

        protected HtmlGenericControl? ContentWrapper { get; set; }
        protected HtmlGenericControl? GoToFirstPageButton { get; set; }
        protected HtmlGenericControl? GoToPreviousPageButton { get; set; }
        protected PlaceHolder? NumberButtonsPlaceHolder { get; set; }
        protected HtmlGenericControl? GoToNextPageButton { get; set; }
        protected HtmlGenericControl? GoToLastPageButton { get; set; }

        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            DataBind(context);
            base.OnLoad(context);
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            DataBind(context);
            base.OnPreRender(context);
        }

        protected virtual void DataBind(Hosting.IDotvvmRequestContext context)
        {
            Children.Clear();

            var dataContextType = DataContextStack.Create(typeof(IPageableGridViewDataSet), this.GetDataContextType());
            ContentWrapper = CreateWrapperList(dataContextType);
            Children.Add(ContentWrapper);

            var bindings = context.Services.GetRequiredService<CommonBindings>();

            object enabledValue = (GetValueBinding(EnabledProperty) is IValueBinding enabledBinding ?
                (object)ValueBindingExpression.CreateBinding<bool>(
                    bindingService.WithoutInitialization(),
                    h => (bool)enabledBinding.Evaluate(this)!,
                    new JsSymbolicParameter(JavascriptTranslator.KnockoutContextParameter).Member("$pagerEnabled")) :
                Enabled);

            
            GoToFirstPageButton = CreateNavigationButton("««", FirstPageTemplate,enabledValue, bindings.GoToFirstPageCommand,context);
            ContentWrapper.Children.Add(GoToFirstPageButton);

            GoToPreviousPageButton = CreateNavigationButton("«", PreviousPageTemplate,enabledValue, bindings.GoToPrevPageCommand,context);
            ContentWrapper.Children.Add(GoToPreviousPageButton);

            // number fields
            NumberButtonsPlaceHolder = new PlaceHolder();
            ContentWrapper.Children.Add(NumberButtonsPlaceHolder);

            var dataSet = DataSet;
            if (dataSet != null)
            {
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
                    NumberButtonsPlaceHolder.Children.Add(li);

                    i++;
                }
            }

            GoToNextPageButton = CreateNavigationButton("»", NextPageTemplate, enabledValue, bindings.GoToNextPageCommand, context);
            ContentWrapper.Children.Add(GoToNextPageButton);

            GoToLastPageButton = CreateNavigationButton("»»", LastPageTemplate, enabledValue, bindings.GoToLastPageCommand, context);
            ContentWrapper.Children.Add(GoToLastPageButton);
        }

        protected virtual HtmlGenericControl CreateWrapperList(DataContextStack dataContext)
        {
            var list = new HtmlGenericControl("ul");
            list.SetDataContextType(dataContext);
            list.SetBinding(DataContextProperty, GetDataSetBinding());
            return list;
        }

        protected virtual HtmlGenericControl CreateNavigationButton(string defaultText, ITemplate? userDefinedContentTemplate, object enabledValue, ICommandBinding clickCommandBindingExpression,IDotvvmRequestContext context)
        {
            var li = new HtmlGenericControl("li");
            var link = new LinkButton();
            SetButtonContent(context, link, defaultText, userDefinedContentTemplate);
            link.SetBinding(ButtonBase.ClickProperty, clickCommandBindingExpression);
            if (!true.Equals(enabledValue))
                link.SetValue(LinkButton.EnabledProperty, enabledValue);
            li.Children.Add(link);
            return li;
        }

        protected virtual void SetButtonContent(Hosting.IDotvvmRequestContext context, LinkButton button, string text, ITemplate? contentTemplate)
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

        private ConditionalWeakTable<DotvvmConfiguration, ConcurrentDictionary<int, ValueBindingExpression>> _nearIndexesBindingCache
            = new ConditionalWeakTable<DotvvmConfiguration, ConcurrentDictionary<int, ValueBindingExpression>>();

        private ValueBindingExpression GetNearIndexesBinding(IDotvvmRequestContext context, int i, DataContextStack? dataContext = null)
        {
            return
                _nearIndexesBindingCache.GetOrCreateValue(context.Configuration)
                .GetOrAdd(i, _ =>
                    ValueBindingExpression.CreateBinding(
                        bindingService.WithoutInitialization(),
                        h => ((IPageableGridViewDataSet)h[0]!).PagingOptions.NearPageIndexes[i],
                        dataContext));
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderOnServer)
            {
                throw new DotvvmControlException(this, "The DataPager control cannot be rendered in the RenderSettings.Mode='Server'.");
            }

            base.AddAttributesToRender(writer, context);

            // If Visible property was set to something, it will be overwritten by this. TODO: is it how it should behave?
            if (HideWhenOnlyOnePage)
            {
                if (IsPropertySet(VisibleProperty))
                    throw new Exception("Visible can't be set on a DataPager when HideWhenOnlyOnePage is true. You can wrap it in an element that hide that or set HideWhenOnlyOnePage to false");
                writer.AddKnockoutDataBind("visible", $"ko.unwrap({GetDataSetBinding().GetKnockoutBindingExpression(this)}).PagingOptions().PagesCount() > 1");
            }
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (GetValueBinding(EnabledProperty) is IValueBinding enabledBinding)
            {
                writer.WriteKnockoutDataBindComment("dotvvm_introduceAlias",
                    $"{{ '$pagerEnabled': { enabledBinding.GetKnockoutBindingExpression(this) }}}");
            }

            if (HasBinding(EnabledProperty))
            {
                AddKnockoutDisabledCssDataBind(writer, context, "$pagerEnabled()");
            }
            else
            {
                AddKnockoutDisabledCssDataBind(writer, context, (!Enabled).ToString().ToLower());
            }

            writer.AddKnockoutDataBind("with", this, DataSetProperty, renderEvenInServerRenderingMode: true);
            writer.RenderBeginTag("ul");
        }

        protected virtual void AddItemCssClass(IHtmlWriter writer, IDotvvmRequestContext context)
        {
        }

        protected virtual void AddKnockoutDisabledCssDataBind(IHtmlWriter writer, IDotvvmRequestContext context, string expression)
        {
            writer.AddKnockoutDataBind("css", $"{{ 'disabled': {expression} }}");
        }

        protected virtual void AddKnockoutActiveCssDataBind(IHtmlWriter writer, IDotvvmRequestContext context, string expression)
        {
            writer.AddKnockoutDataBind("css", $"{{ 'active': {expression} }}");
        }

        private static ParametrizedCode currentPageTextJs = new JsBinaryExpression(new JsBinaryExpression(new JsLiteral(1), BinaryOperatorType.Plus, new JsSymbolicParameter(JavascriptTranslator.KnockoutViewModelParameter)), BinaryOperatorType.Plus, new JsLiteral("")).FormatParametrizedScript();

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            AddItemCssClass(writer, context);
            AddKnockoutDisabledCssDataBind(writer, context, "PagingOptions().IsFirstPage()");
            GoToFirstPageButton!.Render(writer, context);

            AddItemCssClass(writer, context);
            AddKnockoutDisabledCssDataBind(writer, context, "PagingOptions().IsFirstPage()");
            GoToPreviousPageButton!.Render(writer, context);

            // render template
            writer.WriteKnockoutForeachComment("PagingOptions().NearPageIndexes");

            // render page number
            NumberButtonsPlaceHolder!.Children.Clear();
            var li = CreatePageNumberButton(writer, context);
            li.Render(writer, context);

            writer.WriteKnockoutDataBindEndComment();

            AddItemCssClass(writer, context);
            AddKnockoutDisabledCssDataBind(writer, context, "PagingOptions().IsLastPage()");
            GoToNextPageButton!.Render(writer, context);

            AddItemCssClass(writer, context);
            AddKnockoutDisabledCssDataBind(writer, context, "PagingOptions().IsLastPage()");
            GoToLastPageButton!.Render(writer, context);
        }

        protected virtual HtmlGenericControl CreatePageNumberButton(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            HtmlGenericControl li;
            var currentPageTextContext = DataContextStack.Create(typeof(int), NumberButtonsPlaceHolder!.GetDataContextType());
            var currentPageTextBinding = ValueBindingExpression.CreateBinding(bindingService.WithoutInitialization(),
                vm => ((int) vm[0]! + 1).ToString(),
                currentPageTextJs,
                currentPageTextContext);

            if (!RenderLinkForCurrentPage)
            {
                writer.AddKnockoutDataBind("visible", "$data == $parent.PagingOptions().PageIndex()");
                AddItemCssClass(writer, context);
                AddKnockoutActiveCssDataBind(writer, context, "$data == $parent.PagingOptions().PageIndex()");
                li = new HtmlGenericControl("li");
                var literal = new Literal();
                literal.DataContext = 0;
                literal.SetDataContextType(currentPageTextContext);

                literal.SetBinding(Literal.TextProperty, currentPageTextBinding);
                li.Children.Add(literal);
                NumberButtonsPlaceHolder!.Children.Add(li);
                li.Render(writer, context);

                writer.AddKnockoutDataBind("visible", "$data != $parent.PagingOptions().PageIndex()");
            }

            AddItemCssClass(writer, context);
            AddKnockoutActiveCssDataBind(writer, context, "$data == $parent.PagingOptions().PageIndex()");
            li = new HtmlGenericControl("li");
            li.SetValue(Internal.PathFragmentProperty, "PagingOptions.NearPageIndexes[$index]");
            var link = new LinkButton();
            li.Children.Add(link);
            link.SetDataContextType(currentPageTextContext);
            link.SetBinding(ButtonBase.TextProperty, currentPageTextBinding);
            link.SetBinding(ButtonBase.ClickProperty, commonBindings.GoToThisPageCommand);
            object enabledValue = GetValueBinding(EnabledProperty) is IValueBinding enabledBinding
                ? (object) ValueBindingExpression.CreateBinding(bindingService.WithoutInitialization(),
                    h => enabledBinding.Evaluate(this),
                    new JsSymbolicParameter(JavascriptTranslator.KnockoutContextParameter).Member("$pagerEnabled"))
                : Enabled;
            if (!true.Equals(enabledValue)) link.SetValue(LinkButton.EnabledProperty, enabledValue);
            NumberButtonsPlaceHolder!.Children.Add(li);
            return li;
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.RenderEndTag();
            if (HasValueBinding(EnabledProperty)) writer.WriteKnockoutDataBindEndComment();
        }

        private IValueBinding GetDataSetBinding()
            => GetValueBinding(DataSetProperty) ?? throw new DotvvmControlException(this, "The DataSet property of the dot:DataPager control must be set!");
    }

}
