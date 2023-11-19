using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.HelperNamespace;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a pager for <see cref="GridViewDataSet{T}"/> that allows the user to append more items to the end of the list.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = nameof(LoadTemplate))]
    public class AppendableDataPager : HtmlGenericControl 
    {
        private readonly GridViewDataSetBindingProvider gridViewDataSetBindingProvider;

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        [DataPagerApi.AddParameterDataContextChange("_dataPager")]
        public ITemplate? LoadTemplate
        {
            get { return (ITemplate?)GetValue(LoadTemplateProperty); }
            set { SetValue(LoadTemplateProperty, value); }
        }
        public static readonly DotvvmProperty LoadTemplateProperty
            = DotvvmProperty.Register<ITemplate, AppendableDataPager>(c => c.LoadTemplate, null);

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate? EndTemplate
        {
            get { return (ITemplate?)GetValue(EndTemplateProperty); }
            set { SetValue(EndTemplateProperty, value); }
        }
        public static readonly DotvvmProperty EndTemplateProperty
            = DotvvmProperty.Register<ITemplate, AppendableDataPager>(c => c.EndTemplate, null);

        [MarkupOptions(Required = true, AllowHardCodedValue = false)]
        public IPageableGridViewDataSet DataSet
        {
            get { return (IPageableGridViewDataSet)GetValue(DataSetProperty)!; }
            set { SetValue(DataSetProperty, value); }
        }
        public static readonly DotvvmProperty DataSetProperty
            = DotvvmProperty.Register<IPageableGridViewDataSet, AppendableDataPager>(c => c.DataSet, null);

        public ICommandBinding? LoadData
        {
            get => (ICommandBinding?)GetValue(LoadDataProperty);
            set => SetValue(LoadDataProperty, value);
        }
        public static readonly DotvvmProperty LoadDataProperty =
            DotvvmProperty.Register<ICommandBinding?, AppendableDataPager>(nameof(LoadData));


        private DataPagerBindings? dataPagerCommands = null;


        public AppendableDataPager(GridViewDataSetBindingProvider gridViewDataSetBindingProvider) : base("div")
        {
            this.gridViewDataSetBindingProvider = gridViewDataSetBindingProvider;
        }

        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            var dataSetBinding = GetValueBinding(DataSetProperty)!;
            var commandType = LoadData is { } ? GridViewDataSetCommandType.LoadDataDelegate : GridViewDataSetCommandType.Default;
            dataPagerCommands = gridViewDataSetBindingProvider.GetDataPagerCommands(this.GetDataContextType()!, dataSetBinding, commandType);

            if (LoadTemplate != null)
            {
                LoadTemplate.BuildContent(context, this);
            }

            if (EndTemplate != null)
            {
                var container = new HtmlGenericControl("div")
                    .SetProperty(p => p.Visible, dataPagerCommands.IsLastPage);
                Children.Add(container);

                EndTemplate.BuildContent(context, container);
            }
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var dataSetBinding = GetDataSetBinding().GetKnockoutBindingExpression(this, unwrapped: true);

            var helperBinding = new KnockoutBindingGroup();
            helperBinding.Add("dataSet", dataSetBinding);
            if (this.LoadData is { } loadData)
            {
                helperBinding.Add("loadDataSet", KnockoutHelper.GenerateClientPostbackLambda("LoadDataCore", loadData, this, new PostbackScriptOptions(elementAccessor: "$element", koContext: CodeParameterAssignment.FromIdentifier("$context"))));
                helperBinding.Add("loadNextPage", KnockoutHelper.GenerateClientPostbackLambda("LoadData", dataPagerCommands!.GoToNextPage!, this));
                helperBinding.Add("postProcessor", "dotvvm.dataSet.postProcessors.append");
            }
            writer.AddKnockoutDataBind("dotvvm-gridviewdataset", helperBinding.ToString());
            
            var binding = new KnockoutBindingGroup();
            binding.Add("autoLoadWhenInViewport", LoadTemplate == null ? "true" : "false");
            writer.AddKnockoutDataBind("dotvvm-appendable-data-pager", binding);

            base.AddAttributesToRender(writer, context);
        }

        private IValueBinding GetDataSetBinding()
            => GetValueBinding(DataSetProperty) ?? throw new DotvvmControlException(this, "The DataSet property of the dot:DataPager control must be set!");
    }
}
