using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand
{
	public class StaticCommand_ComboBoxSelectionChanged_ObjectsViewModel : DotvvmViewModelBase
	{


        public SelectedValue SelectedValue1 { get; set; } = new SelectedValue();

        public SelectedValue SelectedValue2 { get; set; } = new SelectedValue();

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
        public static SelectedValue Function2(SelectedValue selVal)
        {
            return new SelectedValue()
            {
                Value = selVal.Value * 2
            };
        }

    }

    public class SelectedValue
    {

        public int? Value { get; set; }

    }
}

