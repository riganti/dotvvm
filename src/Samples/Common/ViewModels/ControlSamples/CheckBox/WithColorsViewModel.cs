using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.CheckBox
{
    public class WithColorsViewModel : DotvvmViewModelBase
    {
        public List<CheckBoxColorContext> CheckBoxes { get; set; }

        public List<string> Colors { get; set; } = new List<string>();

        public string SelectedColors { get; set; }

        public override Task PreRender()
        {
            CheckBoxes = new List<CheckBoxColorContext>();
            CheckBoxes = GetData();
            return base.PreRender();
        }

        public void UpdateSelectedColors()
        {
            SelectedColors = string.Join(", ", Colors);
        }

        public void SetCheckedItems()
        {
            Colors = new List<string>() { "orange", "black" };
            UpdateSelectedColors();
        }

        private List<CheckBoxColorContext> GetData()
        {
            return new List<CheckBoxColorContext>
            {
                new CheckBoxColorContext()
                {
                    Text="CheckBox 1",
                    Checked = false,
                    Visible = true,
                    CheckedColor = "orange"
                },
                new CheckBoxColorContext()
                {
                    Text="CheckBox 2",
                    Checked = false,
                    Visible = true,
                    CheckedColor = "red"
                },
                new CheckBoxColorContext()
                {
                    Text="CheckBox 3",
                    Checked = false,
                    Visible = true,
                    CheckedColor = "black"
                },new CheckBoxColorContext()
                {
                    Text="CheckBox 3",
                    Checked = true,
                    Visible = true,
                    CheckedColor = "green"
                }
            };
        }

        public class CheckBoxColorContext
        {
            public string Text { get; set; }
            public bool Checked { get; set; }
            public bool Visible { get; set; }
            public string CheckedColor { get; set; }
        }
    }
}

