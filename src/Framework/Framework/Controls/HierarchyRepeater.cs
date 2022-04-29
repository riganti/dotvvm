using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

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

        public HtmlCapability LevelHtmlCapability
        {
            get => (HtmlCapability)this.GetValue(Level_HtmlCapabilityProperty)!;
            set => this.SetValue(Level_HtmlCapabilityProperty, value);
        }
        public static readonly DotvvmCapabilityProperty Level_HtmlCapabilityProperty =
            DotvvmCapabilityProperty.RegisterCapability<HtmlCapability, HierarchyRepeater>("Level:");

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
                this.AppendChildren(CreateServerLevel(context, GetIEnumerableFromDataSource()!));
            }

            if (renderClientTemplate)
            {
                // whenever possible, we use the dotvvm deterministic ids, but if we are in a client-side template,
                // we'd get a binding... so we just generate a random Guid, not ideal but it will work.
                var uniqueId = GetDotvvmUniqueId() is string id ? id : Guid.NewGuid().ToString();
                clientItemTemplateId = $"{uniqueId}-item";
                clientItemTemplate = GetClientItemTemplate(context);
                context.ResourceManager.AddTemplateResource(context, clientItemTemplate, clientItemTemplateId);
                if (string.IsNullOrEmpty(LevelTagName))
                {
                    clientRootLevel = new PlaceHolder();
                }
                else
                {
                    clientRootLevel = new HtmlGenericControl(LevelTagName, LevelHtmlCapability);
                }

                Children.Add(clientRootLevel);
                clientRootLevel.AppendChildren(new HierarchyRepeaterLevel {
                    IsRoot = true,
                    ForeachExpression = GetForeachDataBindExpression().GetKnockoutBindingExpression(this),
                    ItemTemplateId = clientItemTemplateId,
                });
            }

            if (EmptyDataTemplate is not null)
            {
                Children.Add(emptyDataContainer ??= GetEmptyItem(context));
            }
        }

        private DotvvmControl CreateServerLevel(
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
            DotvvmControl? levelWrapper;
            if (string.IsNullOrEmpty(LevelTagName))
            {
                levelWrapper = new PlaceHolder();
            }
            else
            {
                levelWrapper = new HtmlGenericControl(LevelTagName, LevelHtmlCapability);
            }
            level.Children.Add(levelWrapper);
            {
                var index = 0;
                foreach (var item in items)
                {
                    levelWrapper.AppendChildren(CreateServerItem(context, item, parentPath, index));
                    index++;
                }
            }
            return level;
        }

        private DotvvmControl CreateServerItem(
            IDotvvmRequestContext context,
            object item,
            ImmutableArray<int> parentPath,
            int index)
        {
            DotvvmControl itemWrapper = string.IsNullOrEmpty(ItemTagName)
                ? new PlaceHolder()
                : new HtmlGenericControl(ItemTagName);
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
            }
            // if the item has children then recurse down
            var itemChildren = GetItemChildren(item);
            if (itemChildren.Any())
            {
                var foreachExpression = ((IValueBinding)ItemChildrenBinding!
                    .GetProperty<DataSourceAccessBinding>()
                    .Binding)
                    .GetParametrizedKnockoutExpression(dataItem)
                    .ToString(p =>
                        p == JavascriptTranslator.KnockoutViewModelParameter ? CodeParameterAssignment.FromIdentifier($"ko.unwrap($foreachCollectionSymbol)[{index}]()") :
                        p is JavascriptTranslator.ViewModelSymbolicParameter vm ?
                            JavascriptTranslator.GetKnockoutViewModelParameter(vm.ParentIndex - 1).DefaultAssignment :
                        default
                    );

                itemWrapper.AppendChildren(
                    CreateServerLevel(context, itemChildren, parentPath.Add(index), foreachExpression)
                );
            }
            return itemWrapper;
        }

        private DotvvmControl GetClientItemTemplate(IDotvvmRequestContext context)
        {
            var dataItem = new DataItemContainer();
            {
                dataItem.DataContext = null;
                dataItem.SetValue(Internal.PathFragmentProperty, $"{GetPathFragmentExpression()}/[$indexPath]");
                var bindingService = context.Services.GetRequiredService<BindingCompilationService>();
                dataItem.SetDataContextTypeFromDataSource(GetDataSourceBinding());
                var clientIdFragmentProperty = ValueBindingExpression.CreateBinding<string?>(
                    bindingService.WithoutInitialization(),
                    h => null,
                    new ParametrizedCode("$indexPath.map(ko.unwrap).join(\"_\")", OperatorPrecedence.Max),
                    dataItem.GetDataContextType());
                dataItem.SetValue(Internal.ClientIDFragmentProperty, clientIdFragmentProperty);

                DotvvmControl itemWrapper = string.IsNullOrEmpty(ItemTagName)
                    ? new PlaceHolder()
                    : new HtmlGenericControl(ItemTagName);
                dataItem.Children.Add(itemWrapper);
                // TODO: this is horrible, find a better way to render the begin and end tags
                itemWrapper.AppendChildren(new HtmlLiteral {
                    RenderWrapperTag = false,
                    Html = "<!-- ko with: $item -->"
                });

                ItemTemplate.BuildContent(context, itemWrapper);

                itemWrapper.AppendChildren(new HtmlLiteral {
                    RenderWrapperTag = false,
                    Html = "<!-- /ko -->"
                });

                var foreachExpression = ((IValueBinding)ItemChildrenBinding!
                        .GetProperty<DataSourceAccessBinding>()
                        .Binding)
                        .GetParametrizedKnockoutExpression(dataItem)
                        .ToString(p =>
                            p == JavascriptTranslator.KnockoutViewModelParameter ? CodeParameterAssignment.FromIdentifier("$item()") :
                            p is JavascriptTranslator.ViewModelSymbolicParameter vm ?
                                JavascriptTranslator.GetKnockoutViewModelParameter(vm.ParentIndex - 1).DefaultAssignment :
                            default
                        );

                DotvvmControl? levelWrapper = null;
                if (string.IsNullOrEmpty(LevelTagName))
                {
                    levelWrapper = new PlaceHolder();
                }
                else
                {
                    levelWrapper = new HtmlGenericControl(LevelTagName, LevelHtmlCapability);
                }

                itemWrapper.Children.Add(levelWrapper);

                var level = new HierarchyRepeaterLevel {
                    ItemTemplateId = clientItemTemplateId,
                    ForeachExpression = foreachExpression
                };
                levelWrapper.Children.Add(level);
                level.SetProperty(RenderSettings.ModeProperty, new ValueOrBinding<RenderMode>(RenderMode.Client));
                level.SetProperty(IncludeInPageProperty,
                    (IValueBinding)ItemChildrenBinding!.GetProperty<DataSourceLengthBinding>().Binding);
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

        private IEnumerable<object> GetItemChildren(object item)
        {
            var tempContainer = new DataItemContainer { Parent = this, DataContext = item };
            tempContainer.SetDataContextTypeFromDataSource(GetDataSourceBinding());
            return ItemChildrenBinding!.Evaluate(tempContainer) ?? Enumerable.Empty<object>();
        }

        /// <summary>
        /// A container for a level of the <see cref="HierarchyRepeater"/>.
        /// </summary>
        private class HierarchyRepeaterLevel : DotvvmControl
        {
            public string? ItemTemplateId { get; set; }

            public string? ForeachExpression { get; set; }

            public bool IsRoot { get; set; } = false;

            protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
            {
                if (ForeachExpression is null)
                {
                    base.RenderControl(writer, context);
                    return;
                }

                if (!RenderOnServer)
                {
                    var koGroup = new KnockoutBindingGroup {
                        { "foreach", ForeachExpression }
                    };
                    koGroup.AddValue("name", ItemTemplateId.NotNull());
                    koGroup.AddValue("hierarchyRole", IsRoot ? "Root" : "Child");
                    writer.WriteKnockoutDataBindComment("template", koGroup.ToString());
                }
                else
                {
                    writer.WriteKnockoutDataBindComment("dotvvm-SSR-foreach", new KnockoutBindingGroup {
                        { "data", ForeachExpression}
                    }.ToString());
                }

                base.RenderControl(writer, context);

                writer.WriteKnockoutDataBindEndComment();
            }
        }
    }
}
