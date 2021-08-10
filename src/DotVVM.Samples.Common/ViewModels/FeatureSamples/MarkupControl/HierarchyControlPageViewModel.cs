using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.Common.Views.FeatureSamples.MarkupControl;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.MarkupControl
{
    public class HierarchyControlPageViewModel : DotvvmViewModelBase
    {
        public HierarchicalItem Item { get; set; } = new HierarchicalItem {
            Title = "A",
            Children = {
                new HierarchicalItem {
                    Title = "A-A"
                },
                new HierarchicalItem {
                    Title = "A-B",
                    Children = {
                        new HierarchicalItem {
                            Title = "A-B-C"
                        }
                    }
                },
                new HierarchicalItem {
                    Title = "A-C"
                }
            }
        };

        public string PrefixText { get; set; } = "Default Text";
        public string NewTitle { get; set; } = "";
    }
}

