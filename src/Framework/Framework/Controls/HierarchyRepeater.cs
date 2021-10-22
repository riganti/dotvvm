using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Repeats a template for each item in the hierarchical DataSource collection.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = nameof(ItemTemplate))]
    public class HierarchyRepeater : HierarchyItemsControl
    {
        private readonly BindingCompilationService bindingService;

        private EmptyData? emptyDataContainer;
        private DotvvmControl? clientItemTemplate;
        private string clientItemTemplateId;

        public HierarchyRepeater(BindingCompilationService bindingService) : base("div")
        {
            SetValue(Internal.IsNamingContainerProperty, true);
            this.bindingService = bindingService;
            clientItemTemplateId = "TODO_item";
        }

        /// <summary>
        /// Gets or sets the template for each HierarchyRepeater item.
        /// </summary>
        [CollectionElementDataContextChange(1)]
        [ControlPropertyBindingDataContextChange(nameof(DataSource))]
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement, Required = true)]
        public ITemplate ItemTemplate
        {
            get => (ITemplate)GetValue(ItemTemplateProperty)!;
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DotvvmProperty ItemTemplateProperty =
            DotvvmProperty.Register<ITemplate, HierarchyRepeater>(t => t.ItemTemplate);

        /// <summary>
        /// Gets or sets the template which will be displayed when the data source is empty.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate? EmptyDataTemplate
        {
            get => (ITemplate?)GetValue(EmptyDataTemplateProperty);
            set => SetValue(EmptyDataTemplateProperty, value);
        }

        public static readonly DotvvmProperty EmptyDataTemplateProperty =
            DotvvmProperty.Register<ITemplate?, HierarchyRepeater>(t => t.EmptyDataTemplate);

        /// <summary>
        /// Gets or sets an action used to build the element that wraps each hierarchy level (eg. <c>ul</c>).
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public Action<HtmlGenericControl>? BuildLevelWrapper { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag that wraps each hierarchy level (eg. <c>ul</c>).
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string? LevelTagName
        {
            get => (string?)GetValue(LevelTagNameProperty);
            set => SetValue(LevelTagNameProperty, value);
        }

        public static readonly DotvvmProperty LevelTagNameProperty =
            DotvvmProperty.Register<string?, HierarchyRepeater>(t => t.LevelTagName);

        /// <summary>
        /// Gets or sets an action used to build the element that wraps each hierarchy item (eg. <c>li</c>).
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public Action<DataItemContainer, HtmlGenericControl>? BuildItemWrapper { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag that wraps each hierarchy item (eg. <c>li</c>).
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string? ItemTagName
        {
            get => (string?)GetValue(ItemTagNameProperty);
            set => SetValue(ItemTagNameProperty, value);
        }

        public static readonly DotvvmProperty ItemTagNameProperty
            = DotvvmProperty.Register<string, HierarchyRepeater>(c => c.ItemTagName);

        /// <summary>
        /// Gets or sets whether the control should render a wrapper element.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool RenderWrapperTag
        {
            get { return (bool)GetValue(RenderWrapperTagProperty)!; }
            set { SetValue(RenderWrapperTagProperty, value); }
        }

        public static readonly DotvvmProperty RenderWrapperTagProperty =
            DotvvmProperty.Register<bool, HierarchyRepeater>(t => t.RenderWrapperTag, true);

        /// <summary>
        /// Gets or sets the name of the tag that wraps the <see cref="HierarchyRepeaterLevel"/>.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string WrapperTagName
        {
            get { return (string)GetValue(WrapperTagNameProperty)!; }
            set { SetValue(WrapperTagNameProperty, value); }
        }

        public static readonly DotvvmProperty WrapperTagNameProperty =
            DotvvmProperty.Register<string, HierarchyRepeater>(t => t.WrapperTagName, "div");

        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            if (context.IsPostBack)
            {
                SetChildren(context, renderClientTemplate: false);
            }
            base.OnLoad(context);
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            SetChildren(context, renderClientTemplate: !RenderOnServer);
            base.OnPreRender(context);
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            TagName = RenderWrapperTag ? WrapperTagName : null;

            base.RenderBeginTag(writer, context);
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderOnServer)
            {
                base.RenderContents(writer, context);
            }
            else
            {
                AddTemplateResource(context, clientItemTemplate!, clientItemTemplateId);
                var group = new KnockoutBindingGroup {
                    { "foreach", GetForeachDataBindExpression().GetKnockoutBindingExpression(this) },
                    { "name", clientItemTemplateId, true }
                };
                writer.AddKnockoutDataBind("template", group);
                DotvvmControl rootLevelWrapper = LevelTagName is null
                    ? new PlaceHolder()
                    : new HtmlGenericControl(LevelTagName);
                if (LevelTagName is not null && BuildLevelWrapper is not null)
                    BuildLevelWrapper((HtmlGenericControl)rootLevelWrapper);
                rootLevelWrapper.Render(writer, context);
            }
        }

        /// <summary>
        /// Performs the data-binding and builds controls inside the <see cref="HierarchyRepeater" />.
        /// </summary>
        private void SetChildren(IDotvvmRequestContext context, bool renderClientTemplate)
        {
            Children.Clear();
            emptyDataContainer = null;
            clientItemTemplate = null;

            if (DataSource is not null)
            {
                Children.Add(GetServerLevel(context, GetIEnumerableFromDataSource()!));
            }

            if (renderClientTemplate)
            {
                Children.Add(clientItemTemplate = GetClientItemTemplate(context));
            }

            if (EmptyDataTemplate is not null)
            {
                Children.Add(GetEmptyItem(context));
            }
        }

        private HierarchyRepeaterLevel GetServerLevel(
            IDotvvmRequestContext context,
            IEnumerable items,
            ImmutableArray<int> parentPath = default)
        {
            if (parentPath.IsDefault)
            {
                parentPath = ImmutableArray<int>.Empty;
            }

            var level = new HierarchyRepeaterLevel {
                ForeachExpression = parentPath.IsEmpty
                    ? GetForeachDataBindExpression()
                    : (IValueBinding)ItemChildrenBinding!.GetProperty<DataSourceAccessBinding>().Binding
            };
            {

                DotvvmControl levelWrapper = string.IsNullOrEmpty(LevelTagName)
                            ? new PlaceHolder()
                            : new HtmlGenericControl(LevelTagName);
                level.Children.Add(levelWrapper);
                {
                    var index = 0;
                    foreach (var item in items)
                    {
                        levelWrapper.Children.Add(GetServerItem(context, item, parentPath, index));
                        index++;
                    }
                }
            }

            return level;
        }

        private DataItemContainer GetServerItem(
            IDotvvmRequestContext context,
            object item,
            ImmutableArray<int> parentPath,
            int index)
        {
            // create a data item container and the actual item
            var dataItem = new DataItemContainer { DataItemIndex = index, DataContext = item };
            {
                dataItem.SetValue(
                    Internal.PathFragmentProperty,
                    $"{GetPathFragmentExpression()}/[{string.Join("]/[", parentPath)}]/[{index}]");
                dataItem.SetDataContextTypeFromDataSource(GetDataSourceBinding());
                DotvvmControl itemWrapper = string.IsNullOrEmpty(ItemTagName)
                    ? new PlaceHolder()
                    : new HtmlGenericControl(ItemTagName);
                ItemTemplate.BuildContent(context, itemWrapper);
                dataItem.Children.Add(itemWrapper);

                // if the item has children then recurse down
                var itemChildren = GetItemChildren(dataItem);
                if (itemChildren.Any())
                {
                    itemWrapper.Children.Add(GetServerLevel(context, itemChildren, parentPath.Add(index)));
                }
            }

            return dataItem;
        }

        private DotvvmControl GetClientItemTemplate(IDotvvmRequestContext context)
        {
            var dataItem = new DataItemContainer();
            {
                dataItem.DataContext = null;
                dataItem.SetValue(Internal.PathFragmentProperty, $"{GetPathFragmentExpression()}/[$indexPath]");
                dataItem.SetValue(Internal.ClientIDFragmentProperty, "$indexPath.map(function(i){return i();}).join(\"_\")");
                dataItem.SetDataContextTypeFromDataSource(GetDataSourceBinding());
                DotvvmControl itemWrapper = string.IsNullOrEmpty(ItemTagName)
                    ? new PlaceHolder()
                    : new HtmlGenericControl(ItemTagName);
                dataItem.Children.Add(itemWrapper);
                ItemTemplate.BuildContent(context, itemWrapper);

                var level = new HierarchyRepeaterLevel {
                    ItemTemplateId = clientItemTemplateId,
                    ForeachExpression = (IValueBinding)ItemChildrenBinding!.GetProperty<DataSourceAccessBinding>().Binding
                };
                level.SetProperty(RenderSettings.ModeProperty, RenderMode.Client);
                level.SetProperty(IncludeInPageProperty, (IValueBinding)ItemChildrenBinding!.GetProperty<DataSourceLengthBinding>().Binding);
                itemWrapper.Children.Add(level);

                DotvvmControl levelWrapper = string.IsNullOrEmpty(LevelTagName)
                    ? new PlaceHolder()
                    : new HtmlGenericControl(LevelTagName);
                level.Children.Add(levelWrapper);
            }

            return dataItem;
        }

        private DotvvmControl GetEmptyItem(IDotvvmRequestContext context)
        {
            if (emptyDataContainer is null)
            {
                var dataSourceBinding = GetDataSourceBinding();
                emptyDataContainer = new EmptyData();
                emptyDataContainer.SetValue(EmptyData.RenderWrapperTagProperty, !string.IsNullOrEmpty(LevelTagName));
                emptyDataContainer.SetValue(EmptyData.WrapperTagNameProperty, GetValueRaw(LevelTagNameProperty));
                emptyDataContainer.SetValue(EmptyData.VisibleProperty, GetValueRaw(VisibleProperty));
                emptyDataContainer.SetBinding(DataSourceProperty, dataSourceBinding);
                EmptyDataTemplate!.BuildContent(context, emptyDataContainer);
            }
            return emptyDataContainer;
        }

        private void AddTemplateResource(IDotvvmRequestContext context, DotvvmControl template, string id)
        {
            if (context.ResourceManager.RequiredResources.Contains(id))
            {
                return;
            }

            using var text = new StringWriter();
            template.Render(new HtmlWriter(text, context), context);
            context.ResourceManager.AddRequiredResource(id, new TemplateResource(text.ToString()));
        }
    }
}
