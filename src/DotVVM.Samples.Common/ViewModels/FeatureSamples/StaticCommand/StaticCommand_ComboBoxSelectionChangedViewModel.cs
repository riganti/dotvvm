using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand
{
	public class StaticCommand_ComboBoxSelectionChangedViewModel : DotvvmViewModelBase
	{

        public int? SelectedValue1 { get; set; }

        public int? SelectedValue2 { get; set; }

        public SelectListItem[] Select1 { get; set; } = 
        {
            new SelectListItem() { Text = "" },
            new SelectListItem() { Text = "Value1", Id = 1 },
            new SelectListItem() { Text = "Value2", Id = 2 }
        };

        public SelectListItem[] Select2 { get; set; } = 
        {
            new SelectListItem() { Text = "" },
            new SelectListItem() { Text = "Value2", Id = 2 },
            new SelectListItem() { Text = "Value4", Id = 4 }
        };

        [AllowStaticCommand]
        public static int? Function2(int? selVal)
        {
            return selVal * 2;
        }


    }

    public class SelectListItem
    {
        public int? Id { get; set; }
        public string Text { get; set; }
    }

}
