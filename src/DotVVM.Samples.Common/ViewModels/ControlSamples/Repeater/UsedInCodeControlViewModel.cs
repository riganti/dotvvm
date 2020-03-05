using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.Repeater
{
    public class UsedInCodeControlViewModel : DotvvmViewModelBase
    {

        public List<MenuItem> HeaderMenu { get; set; } = new List<MenuItem>()
        {
            new MenuItem() { Id = "1", Text = "One" },
            new MenuItem() { Id = "2", Text = "Two" },
            new MenuItem() { Id = "3", Text = "Three" }
        };

        public string SelectedMenuId { get; set; }

        public string Result { get; set; }

        public void HeaderMenuChanged()
        {
            Result = SelectedMenuId ?? "null";
        }
    }

    public class MenuItem
    {
        public string Id { get; set; }

        public string Text { get; set; }
    }
}

