using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.BindingContexts
{
    public class BindingContextViewModel : DotvvmViewModelBase
    {
        public string Result { get; set; }

        public List<BindingContextChildViewModel> Children { get; set; }

        public GridViewDataSet<string> ChildrenDataSet { get; set; }

        public void Test(string value)
        {
            Result = value;
        }

        public override Task PreRender()
        {
            if (!Context.IsPostBack)
            {
                Children = new List<BindingContextChildViewModel>
                {
                    new BindingContextChildViewModel
                    {
                        Children = new List<BindingContextChildViewModel>
                        {
                            new BindingContextChildViewModel
                            {
                                Children = new List<BindingContextChildViewModel>
                                {
                                    new BindingContextChildViewModel
                                    {
                                        Children = new List<BindingContextChildViewModel>()
                                    }
                                }
                            }
                        }
                    }
                };

                ChildrenDataSet = new GridViewDataSet<string>
                {
                    Items = new List<string> {"test"},
                    Pager =
                    {
                        TotalItemsCount = 1
                    }
                };
            }

            return base.PreRender();
        }
    }

    public class BindingContextChildViewModel
    {
        public List<BindingContextChildViewModel> Children { get; set; }
    }
}
