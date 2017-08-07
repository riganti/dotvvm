using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.CheckBox
{
    public class CheckBoxViewModel
    {
        public string Text { get; set; } = "Label text";

        public Grenade Bomb { get; set; } = new Grenade {Name = "McXplode"};

        public bool Checked { get; set; }

        public bool? Indeterminate { get; set; }

        public string CheckedDescription { get; set; }

        public List<string> CheckedItems { get; set; } = new List<string>();

        public string CheckedItemsDescription { get; set; }

        public bool Visible { get; set; } = true;

        public bool ChangedValue { get; set; }

        public int NumberOfChanges { get; set; }

        public void UpdateCheckedItemsDescription()
        {
            CheckedItemsDescription = string.Join(", ", CheckedItems.Select(i => i.ToString()));
        }

        public void UpdateCheckedDescription()
        {
            CheckedDescription = Checked.ToString();
        }

        public void OnChanged()
        {
            NumberOfChanges++;
        }

        public class Grenade
        {
            public string Name { get; set; }

            public bool HasPin { get; set; }
        }
    }
}