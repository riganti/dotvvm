using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.CheckBox
{
    public class CheckboxCheckedItemsNullCollectionViewModel : DotvvmViewModelBase
    {
        public override Task PreRender()
        {
            CheckBoxes = new List<CheckBoxViewModel2>();
            CheckBoxes = GetData();
            return base.PreRender();
        }

        public List<CheckBoxViewModel2> CheckBoxes { get; set; }

        public List<string> Colors { get; set; }

        public void UpdateSelectedColors()
        {
            SelectedColors = string.Join(", ", Colors);
        }

        public string SelectedColors { get; set; }

        public void SetCheckedItems()
        {
            Colors = new List<string>() { "one", "three" };
            UpdateSelectedColors();
        }
        private List<CheckBoxViewModel2> GetData()
        {
            return new List<CheckBoxViewModel2>
            {
                new CheckBoxViewModel2()
                {
                    Text="CheckBox 1",
                    Checked = false,
                    Visible = true,
                    CheckedValue = "one"
                },
                new CheckBoxViewModel2()
                {
                    Text="CheckBox 2",
                    Checked = false,
                    Visible = true,
                    CheckedValue = "two"
                },
                new CheckBoxViewModel2()
                {
                    Text="CheckBox 3",
                    Checked = false,
                    Visible = true,
                    CheckedValue = "three"
                },new CheckBoxViewModel2()
                {
                    Text="CheckBox 3",
                    Checked = true,
                    Visible = true,
                    CheckedValue = "four"
                }
            };
        }

    }
}

