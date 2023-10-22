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
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the pagination control which can be integrated with the GridViewDataSet object to provide the paging capabilities.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class DataPager : HtmlGenericControl
    {
        private readonly GridViewDataSetBindingProvider gridViewDataSetBindingProvider;
        private readonly BindingCompilationService bindingCompilationService;

        private DataPagerBindings? pagerBindings;


        public DataPager(GridViewDataSetBindingProvider gridViewDataSetBindingProvider, BindingCompilationService bindingCompilationService)
            : base("ul", false)
        {
            this.gridViewDataSetBindingProvider = gridViewDataSetBindingProvider;
            this.bindingCompilationService = bindingCompilationService;
        }

        /// <summary>
        /// Gets or sets the GridViewDataSet object in the viewmodel.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public IPageableGridViewDataSet<IPagingOptions>? DataSet
        {
            get { return (IPageableGridViewDataSet<IPagingOptions>?)GetValue(DataSetProperty); }
            set { SetValue(DataSetProperty, value); }
        }
        public static readonly DotvvmProperty DataSetProperty =
            DotvvmProperty.Register<IPageableGridViewDataSet<IPagingOptions>?, DataPager>(c => c.DataSet);

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

        public ICommandBinding? LoadData
        {
            get => (ICommandBinding?)GetValue(LoadDataProperty);
            set => SetValue(LoadDataProperty, value);
        }
        public static readonly DotvvmProperty LoadDataProperty =
            DotvvmProperty.Register<ICommandBinding?, DataPager>(nameof(LoadData));

        protected HtmlGenericControl? ContentWrapper { get; set; }
        protected HtmlGenericControl? GoToFirstPageButton { get; set; }
        protected HtmlGenericControl? GoToPreviousPageButton { get; set; }
        protected Repeater? NumberButtonsRepeater { get; set; }
        protected HtmlGenericControl? GoToNextPageButton { get; set; }
        protected HtmlGenericControl? GoToLastPageButton { get; set; }
        protected virtual string ActiveItemCssClass => "active";
        protected virtual string DisabledItemCssClass => "disabled";

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

            var dataSetBinding = GetValueBinding(DataSetProperty)!;
            var dataSetType = dataSetBinding.ResultType;
            ContentWrapper = CreateWrapperList();
            Children.Add(ContentWrapper);

            var commandType = LoadData is {} ? GridViewDataSetCommandType.StaticCommand : GridViewDataSetCommandType.Command;

            pagerBindings = gridViewDataSetBindingProvider.GetDataPagerCommands(this.GetDataContextType().NotNull(), dataSetBinding, commandType);


            var enabled = GetValueOrBinding<bool>(EnabledProperty)!;
            
            if (typeof(IPageableGridViewDataSet<IPagingFirstPageCapability>).IsAssignableFrom(dataSetType))
            {
                GoToFirstPageButton = CreateNavigationButton("««", FirstPageTemplate, enabled, pagerBindings.GoToFirstPage!, context);
                GoToFirstPageButton.CssClasses.Add(DisabledItemCssClass, new ValueOrBinding<bool>(pagerBindings.IsFirstPage.NotNull()));
                ContentWrapper.Children.Add(GoToFirstPageButton);
            }

            if (typeof(IPageableGridViewDataSet<IPagingPreviousPageCapability>).IsAssignableFrom(dataSetType))
            {
                GoToPreviousPageButton = CreateNavigationButton("«", PreviousPageTemplate, enabled, pagerBindings.GoToPreviousPage!, context);
                GoToPreviousPageButton.CssClasses.Add(DisabledItemCssClass, new ValueOrBinding<bool>(pagerBindings.IsFirstPage.NotNull()));
                ContentWrapper.Children.Add(GoToPreviousPageButton);
            }

            if (pagerBindings.PageNumbers is {})
            {
                // number fields
                var liTemplate = new HtmlGenericControl("li");
                // li.SetDataContextType(currentPageTextContext);
                // li.SetBinding(DataContextProperty, GetNearIndexesBinding(context, i, dataContextType));
                liTemplate.CssClasses.Add(ActiveItemCssClass, new ValueOrBinding<bool>(pagerBindings.IsActivePage.NotNull()));
                var link = new LinkButton();
                link.SetBinding(ButtonBase.ClickProperty, pagerBindings.GoToPage!);
                link.SetBinding(ButtonBase.TextProperty, pagerBindings.PageNumberText);
                if (!true.Equals(enabled)) link.SetValue(LinkButton.EnabledProperty, enabled);
                liTemplate.Children.Add(link);
                NumberButtonsRepeater = new Repeater() {
                    DataSource = pagerBindings.PageNumbers,
                    RenderWrapperTag = false,
                    RenderAsNamedTemplate = false,
                    ItemTemplate = new CloneTemplate(liTemplate)
                };
                ContentWrapper.Children.Add(NumberButtonsRepeater);
            }

            if (typeof(IPageableGridViewDataSet<IPagingNextPageCapability>).IsAssignableFrom(dataSetType))
            {
                GoToNextPageButton = CreateNavigationButton("»", NextPageTemplate, enabled, pagerBindings.GoToNextPage!, context);
                GoToNextPageButton.CssClasses.Add(DisabledItemCssClass, new ValueOrBinding<bool>(pagerBindings.IsLastPage.NotNull()));
                ContentWrapper.Children.Add(GoToNextPageButton);
            }

            if (typeof(IPageableGridViewDataSet<IPagingLastPageCapability>).IsAssignableFrom(dataSetType))
            {
                GoToLastPageButton = CreateNavigationButton("»»", LastPageTemplate, enabled, pagerBindings.GoToLastPage!, context);
                GoToLastPageButton.CssClasses.Add(DisabledItemCssClass, new ValueOrBinding<bool>(pagerBindings.IsLastPage.NotNull()));
                ContentWrapper.Children.Add(GoToLastPageButton);
            }
        }

        protected virtual HtmlGenericControl CreateWrapperList()
        {
            var list = new HtmlGenericControl("ul");
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

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderOnServer)
            {
                throw new DotvvmControlException(this, "The DataPager control cannot be rendered in the RenderSettings.Mode='Server'.");
            }

            var dataSetBinding = GetDataSetBinding().GetKnockoutBindingExpression(this, unwrapped: true);
            var helperBinding = new KnockoutBindingGroup();
            helperBinding.Add("dataSet", dataSetBinding);
            if (this.LoadData is {} loadData)
            {
                var loadDataExpression = KnockoutHelper.GenerateClientPostbackLambda("LoadData", loadData, this, new PostbackScriptOptions(elementAccessor: "$element", koContext: CodeParameterAssignment.FromIdentifier("$context")));
                helperBinding.Add("loadDataSet", loadDataExpression);
            }
            writer.AddKnockoutDataBind("dotvvm-gridviewdataset", helperBinding.ToString());

            // If Visible property was set to something, it will be overwritten by this. TODO: is it how it should behave?
            if (HideWhenOnlyOnePage)
            {
                if (IsPropertySet(VisibleProperty))
                    throw new Exception("Visible can't be set on a DataPager when HideWhenOnlyOnePage is true. You can wrap it in an element that hide that or set HideWhenOnlyOnePage to false");
                writer.AddKnockoutDataBind("visible", $"({dataSetBinding}).PagingOptions().PagesCount() > 1");
            }

            if (GetValueBinding(EnabledProperty) is IValueBinding enabledBinding)
            {
                var disabledBinding = enabledBinding.GetProperty<NegatedBindingExpression>().Binding.CastTo<IValueBinding>();
                AddKnockoutDisabledCssDataBind(writer, context, disabledBinding.GetKnockoutBindingExpression(this));
            }
            else if (!Enabled)
            {
                writer.AddAttribute("class", DisabledItemCssClass, true);
            }

            base.AddAttributesToRender(writer, context);
        }

        protected virtual void AddKnockoutDisabledCssDataBind(IHtmlWriter writer, IDotvvmRequestContext context, string expression)
        {
            writer.AddKnockoutDataBind("css", $"{{ '{DisabledItemCssClass}': {expression} }}");
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.RenderContents(writer, context);
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // don't, delegated to the ContentWrapper html element
        }
        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
        }


        private IValueBinding GetDataSetBinding()
            => GetValueBinding(DataSetProperty) ?? throw new DotvvmControlException(this, "The DataSet property of the dot:DataPager control must be set!");
    }
}
