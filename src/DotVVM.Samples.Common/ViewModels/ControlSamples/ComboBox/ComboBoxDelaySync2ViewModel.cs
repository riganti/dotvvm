using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ComboBox
{
	public class ComboBoxDelaySync2ViewModel : DotvvmViewModelBase
	{
        public List<FirstListItem> FirstList { get; set; } = new List<FirstListItem>();
        public List<SecondListItem> SecondList { get; set; } = new List<SecondListItem>();

        public void DoPostback()
        {
            FirstList = new List<FirstListItem>()
            {
                new FirstListItem() { SelectedValue = 1 },
                new FirstListItem() { SelectedValue = 2 }
            };

            SecondList = new List<SecondListItem>()
            {
                new SecondListItem() { Text = "Value1", Value = 1 },
                new SecondListItem() { Text = "Value2", Value = 2 }
            };
        }
    }
    public class SecondListItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }


    public class FirstListItem
    {
        public int SelectedValue { get; set; }
    }
}

