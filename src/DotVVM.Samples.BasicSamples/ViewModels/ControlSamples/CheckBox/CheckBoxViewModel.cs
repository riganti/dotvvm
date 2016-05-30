using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.CheckBox
{
    public class CheckBoxViewModel
    {
        public string Text { get; set; } = "Label text";
        
        public bool SCB { get; set; }

        public string SCBResult { get; set; }

        public void UpdateSCB()
        {
            SCBResult = SCB.ToString();
        }


        public List<string> MCB { get; set; }

        public string MCBResult { get; set; }

        public void UpdateMCB()
        {
            MCBResult = string.Join(", ", MCB.Select(i => i.ToString()));
        }

        public CheckBoxViewModel()
        {
            MCB = new List<string>();
        }



        public bool ChangedValue { get; set; }

        public int NumberOfChanges { get; set; }

        public void OnChanged()
        {
            NumberOfChanges++;
        }
    }
}