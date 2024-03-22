using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.MarkupControl
{
    public class CommandAsPropertyPageViewModel : DotvvmViewModelBase
    {

        public List<ItemModel> Items { get; set; } = new() {
            new ItemModel() { Name = "One", IsTrue = true },
            new ItemModel() { Name = "Two", IsTrue = false },
            new ItemModel() { Name = "Three", IsTrue = true }
        };

        public ItemModel SelectedItem { get; set; }

        public Task MyFunction(string name, bool isTrue)
        {
            SelectedItem = new ItemModel() { Name = name, IsTrue = isTrue };
            return Task.CompletedTask;
        }


        public class ItemModel
        {
            public string Name { get; set; }
            public bool IsTrue { get; set; }

        }
    }
}

