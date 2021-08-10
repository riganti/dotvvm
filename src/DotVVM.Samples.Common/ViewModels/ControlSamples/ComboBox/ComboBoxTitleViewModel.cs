using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ComboBox
{
    public class ComboBoxTitleViewModel : DotvvmViewModelBase
    {
        public int SelectedValue { get; set; }
        public List<ListItem> List { get; set; }

        public override Task Load()
        {
            List = new List<ListItem>()
            {
                new ListItem() { Value = 1, Text= "Too long text", Title = "Nice title" },
                new ListItem() { Value = 2, Text = "Text", Title = "Even nicer title"}
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

