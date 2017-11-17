using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ComboBox
{
    public class ComboBoxDelaySync3ViewModel : DotvvmViewModelBase
    {

        public ComboBoxDelaySync3State Item { get; set; }

        public ComboBoxDelaySync3Value[] Items { get; set; }


        public void Show()
        {
            Item = new ComboBoxDelaySync3State();
            Items = new[]
            {
                new ComboBoxDelaySync3Value() { Value = "a", Id = 1 },
                new ComboBoxDelaySync3Value() { Value = "b", Id = 2 },
                new ComboBoxDelaySync3Value() { Value = "c", Id = 3 },
                new ComboBoxDelaySync3Value() { Value = "d", Id = 4 },
                new ComboBoxDelaySync3Value() { Value = "e", Id = 5 }
            };

            Item.SelectedItem = 1;
        }
        
    }

    public class ComboBoxDelaySync3State
    {

        public int SelectedItem { get; set; }

    }

    public class ComboBoxDelaySync3Value
    {
        public int Id { get; set; }

        public string Value { get; set; }
    }
}
