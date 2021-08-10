using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
    public class BindingSharingViewModel : DotvvmViewModelBase
    {
        public BindingSharingCategoryDTO[] Categories1 { get; set; } = new []
        {
            new BindingSharingCategoryDTO() { Category = 1 },
            new BindingSharingCategoryDTO() { Category = 2 },
            new BindingSharingCategoryDTO() { Category = 3 }
        };

        public BindingSharingCategoryDTO[] Categories2 { get; set; } = new[]
        {
            new BindingSharingCategoryDTO() { Category = 1 },
            new BindingSharingCategoryDTO() { Category = 2 },
            new BindingSharingCategoryDTO() { Category = 3 }
        };

        public BindingSharingCategoryDTO[] Categories3 { get; set; }

        public void LoadCategories3()
        {
            Categories3 = new[]
            {
                new BindingSharingCategoryDTO() { Category = 1 },
                new BindingSharingCategoryDTO() { Category = 2 },
                new BindingSharingCategoryDTO() { Category = 3 }
            };
        }

    }

    public class BindingSharingCategoryDTO
    {
        public int Category { get; set; }

        public int SelectedValue { get; set; }

    }
}
