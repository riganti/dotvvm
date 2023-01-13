using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using System;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ListBox
{
    public class ListBoxViewModel : DotvvmViewModelBase
    {
        public int SelectedValue { get; set; }
        public List<int> SelectedValues { get; set; } = new List<int>();

        public List<ListItem> List { get; set; }

        public override Task Load()
        {
            List = new List<ListItem>()
            {
                new ListItem() { Value = 1, Text= "Too long text", Title = "Nice title" },
                new ListItem() { Value = 2, Text = "Text1", Title = "Even nicer title"},
                new ListItem() { Value = 3, Text = "Text2", Title = "Even nicer title"},
                new ListItem() { Value = 4, Text = "Text3", Title = "Even nicer title"},
                new ListItem() { Value = 5, Text = "Text4", Title = "Even nicer title"}
            };

            return base.Load();
        }

        public class ListItem
        {
            public int Value { get; set; }
            public string Text { get; set; }
            public string Title { get; set; }
        }
    }
}
