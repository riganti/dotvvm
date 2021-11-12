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
    public class HierarchyRepeater : ItemsControl
    {
        private EmptyData? emptyDataContainer;
        private DotvvmControl? clientItemTemplate;
        private DotvvmControl? clientRootLevel;
        private string? clientItemTemplateId;

        public HierarchyRepeater() : base("div")
        {
            SetValue(Internal.IsNamingContainerProperty, true);
        }

        /// <summary>
        /// Gets or sets the binding which retrieves children of a data item.
        /// </summary>
        [CollectionElementDataContextChange(1)]
        [ControlPropertyBindingDataContextChange(nameof(DataSource))]
        [BindingCompilationRequirements(new[] { typeof(DataSourceAccessBinding) }, new[] { typeof(DataSourceLengthBinding) })]
        [MarkupOptions(Required = true)]
        public IValueBinding<IEnumerable<object>>? ItemChildrenBinding
        {
            get => (IValueBinding<IEnumerable<object>>?)GetValue(ItemChildrenBindingProperty);
            set => SetValue(ItemChildrenBindingProperty, value);
        }

        public static readonly DotvvmProperty ItemChildrenBindingProperty
            = DotvvmProperty.Register<IValueBinding<IEnumerable<object>>?, HierarchyRepeater>(t => t.ItemChildrenBinding);

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

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.RenderEndTag(writer, context);
            emptyDataContainer?.Render(writer, context);
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderOnServer)
            {
                foreach (var child in Children.Except(new[] { emptyDataContainer! }))
                {
                    child.Render(writer, context);
                }
            }
            else
            {
                clientRootLevel!.Render(writer, context);
            }
        }

        private void SetChildren(IDotvvmRequestContext context, bool renderClientTemplate)
        {
            Children.Clear();
            emptyDataContainer = null;
            clientItemTemplate = null;

            if (DataSource is not null)
            {
                AddServerLevel(this, context, GetIEnumerableFromDataSource()!);
            }

            if (renderClientTemplate)
            {
                clientItemTemplateId = $"{GetDotvvmUniqueId()}-item";
                clientItemTemplate = GetClientItemTemplate(context);
                AddTemplateResource(context, clientItemTemplate, clientItemTemplateId);
                clientRootLevel = string.IsNullOrEmpty(LevelTagName)
                    ? new PlaceHolder()
                    : new HtmlGenericControl(LevelTagName);
                
                Children.Add(clientRootLevel);
                {
                    var rootLevel = new HierarchyRepeaterLevel {
                        IsRoot = true,
                        ForeachExpression = GetForeachDataBindExpression().GetKnockoutBindingExpression(this),
                        ItemTemplateId = clientItemTemplateId,
                    };
                    clientRootLevel.Children.Add(rootLevel);
                }
            }

            if (EmptyDataTemplate is not null)
            {
                Children.Add(emptyDataContainer ??= GetEmptyItem(context));
            }
        }

        private void AddServerLevel(
            DotvvmControl parent,
            IDotvvmRequestContext context,
            IEnumerable items,
            ImmutableArray<int> parentPath = default,
            string? foreachExpression = default)
        {
            if (parentPath.IsDefault)
            {
                parentPath = ImmutableArray<int>.Empty;
            }

            foreachExpression ??= ((IValueBinding)GetDataSourceBinding()
                .GetProperty<DataSourceAccessBinding>()
                .Binding)
                .GetKnockoutBindingExpression(this);

            var level = new HierarchyRepeaterLevel {
                ForeachExpression = foreachExpression
            };
            parent.Children.Add(level);
            {
                DotvvmControl levelWrapper = string.IsNullOrEmpty(LevelTagName)
                            ? new PlaceHolder()
                            : new HtmlGenericControl(LevelTagName);
                level.Children.Add(levelWrapper);
                {
                    var index = 0;
                    foreach (var item in items)
                    {
                        AddServerItem(levelWrapper, context, item, parentPath, index);
                        index++;
                    }
                }
            }
        }

        private void AddServerItem(
            DotvvmControl parent,
            IDotvvmRequestContext context,
            object item,
            ImmutableArray<int> parentPath,
            int index)
        {
            DotvvmControl itemWrapper = string.IsNullOrEmpty(ItemTagName)
                ? new PlaceHolder()
                : new HtmlGenericControl(ItemTagName);
            parent.Children.Add(itemWrapper);
            {
                var dataItem = new DataItemContainer { DataItemIndex = index };
                itemWrapper.Children.Add(dataItem);
                dataItem.SetDataContextTypeFromDataSource(GetDataSourceBinding());
                {
                    var placeholder = new PlaceHolder { DataContext = item };
                    {
                        var parentSegment = parentPath.IsDefaultOrEmpty
                            ? string.Empty
                            : $"/[{string.Join("]/[", parentPath)}]";
                        placeholder.SetValue(
                            Internal.PathFragmentProperty,
                            $"{GetPathFragmentExpression()}{parentSegment}/[{index}]");
                        ItemTemplate.BuildContent(context, placeholder);
                    }
                    dataItem.Children.Add(placeholder);

                    var foreachExpression = ((IValueBinding)ItemChildrenBinding!
                        .GetProperty<DataSourceAccessBinding>()
                        .Binding)
                        .GetKnockoutBindingExpression(dataItem);

                    // if the item has children then recurse down
                    parentPath = parentPath.Add(index);
                    var itemChildren = GetItemChildren(item);
                    if (itemChildren.Any())
                    {
                        AddServerLevel(dataItem, context, itemChildren, parentPath, foreachExpression);
                    }
                }
            }
        }

        private DotvvmControl GetClientItemTemplate(IDotvvmRequestContext context)
        {
            var dataItem = new DataItemContainer();
            {
                dataItem.DataContext = null;
                dataItem.SetValue(Internal.PathFragmentProperty, $"{GetPathFragmentExpression()}/[$indexPath]");
                dataItem.SetValue(Internal.ClientIDFragmentProperty, "$indexPath.map(function(i){return i();}).join(\"_\")");
                dataItem.SetDataContextTypeFromDataSource(GetDataSourceBinding());

                // TODO: this is horrible, find a better way to render the begin and end tags
                var withBegin = new HtmlLiteral {
                    RenderWrapperTag = false,
                    Html = "<!-- ko with: $item -->"
                };
                dataItem.Children.Add(withBegin);


                DotvvmControl itemWrapper = string.IsNullOrEmpty(ItemTagName)
                    ? new PlaceHolder()
                    : new HtmlGenericControl(ItemTagName);
                dataItem.Children.Add(itemWrapper);
                ItemTemplate.BuildContent(context, itemWrapper);

                var foreachExpression = ((IValueBinding)ItemChildrenBinding!
                        .GetProperty<DataSourceAccessBinding>()
                        .Binding)
                        .GetKnockoutBindingExpression(dataItem);

                DotvvmControl levelWrapper = string.IsNullOrEmpty(LevelTagName)
                    ? new PlaceHolder()
                    : new HtmlGenericControl(LevelTagName);
                itemWrapper.Children.Add(levelWrapper);

                var level = new HierarchyRepeaterLevel {
                    ItemTemplateId = clientItemTemplateId,
                    ForeachExpression = foreachExpression
                };
                levelWrapper.Children.Add(level);
                level.SetProperty(RenderSettings.ModeProperty, new ValueOrBinding<RenderMode>(RenderMode.Client));
                level.SetProperty(IncludeInPageProperty,
                    (IValueBinding)ItemChildrenBinding!.GetProperty<DataSourceLengthBinding>().Binding);

                var withEnd = new HtmlLiteral {
                    RenderWrapperTag = false,
                    Html = "<!-- /ko -->"
                };
                dataItem.Children.Add(withEnd);
            }

            return dataItem;
        }

        private EmptyData GetEmptyItem(IDotvvmRequestContext context)
        {
            var emptyDataContainer = new EmptyData();
            emptyDataContainer.SetValue(EmptyData.RenderWrapperTagProperty, GetValueRaw(RenderWrapperTagProperty));
            emptyDataContainer.SetValue(EmptyData.WrapperTagNameProperty, GetValueRaw(WrapperTagNameProperty));
            emptyDataContainer.SetValue(EmptyData.VisibleProperty, GetValueRaw(VisibleProperty));
            emptyDataContainer.SetBinding(DataSourceProperty, GetDataSourceBinding());
            EmptyDataTemplate!.BuildContent(context, emptyDataContainer);
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

        private IEnumerable<object> GetItemChildren(object item)
        {
            var tempContainer = new DataItemContainer { Parent = this, DataContext = item };
            tempContainer.SetDataContextTypeFromDataSource(GetDataSourceBinding());
            return ItemChildrenBinding!.Evaluate(tempContainer) ?? Enumerable.Empty<object>();
        }
    }
}
