using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;


namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.FormControlsEnabled
{
    public class FormControlsEnabled : DotvvmViewModelBase
    {
        public ChildForm Child { get; set; } = new ChildForm();

        public string[] Items { get; set; } = { "one", "two", "three" };
        public string SelectedItem { get; set; } = "one";
        public bool[] FormsEnabled { get; set; } = { false, true};
        public int LinkButtonsPressed { get; set; } = 0;
        public bool Enabled { get; set; } = false;

        public void Switch() => Enabled = !Enabled;

        public void LinkButtonPressed() => LinkButtonsPressed++;
    }

    public class ChildForm
    {
        public string Text1 { get; set; }
        public string Text2 { get; set; }
    }
}
