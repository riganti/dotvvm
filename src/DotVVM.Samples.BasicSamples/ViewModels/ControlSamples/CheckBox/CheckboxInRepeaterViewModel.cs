using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.CheckBox
{
	public class CheckboxInRepeaterViewModel : DotvvmViewModelBase
	{
        public override Task PreRender()
        {
            CheckBoxes = new List<CheckBox>();
            CheckBoxes = GetData();
            return base.PreRender();
        }

	    public List<CheckBox> CheckBoxes { get; set; }

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
        private List<CheckBox> GetData()
        {
            return new List<CheckBox>
            {
                new CheckBox()
                {
                    Text="CheckBox 1",
                    Checked = false,
                    Visible = true,
                    CheckedValue = "one"
                },
                new CheckBox()
                {
                    Text="CheckBox 2",
                    Checked = false,
                    Visible = true,
                    CheckedValue = "two"
                },
                new CheckBox()
                {
                    Text="CheckBox 3",
                    Checked = false,
                    Visible = true,
                    CheckedValue = "three"
                },new CheckBox()
                {
                    Text="CheckBox 3",
                    Checked = true,
                    Visible = true,
                    CheckedValue = "four"
                }
            };
        }

    }   public class CheckBox
        {
            public string Text { get; set; }
            public bool Checked { get; set; }
            public bool Visible { get; set; }
            public string CheckedValue { get; set; }
        }
}

