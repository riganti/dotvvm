using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.ComboBox
{
    public class NullableViewModel
    {
        public enum SampleEnum
        {
            First,
            Second,
            Third
        }

        public SampleEnum? SelectedValue { get; set; }

        public List<Item> Items { get; set; } = new List<Item> {
            new Item {
                Text = "First",
                Value = SampleEnum.First
            },
            new Item {
                Text = "Second",
                Value = SampleEnum.Second
            },
            new Item {
                Text = "Third",
                Value = SampleEnum.Third
            }
        };

        public class Item
        {
            public string Text { get; set; }

            public SampleEnum Value { get; set; }
        }


        public void SetNull() => SelectedValue = null;
        public void SetFirst() => SelectedValue = SampleEnum.First;
        public void SetSecond() => SelectedValue = SampleEnum.Second;
    }
}
