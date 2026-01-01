using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.CheckBox
{
    public class CheckBoxCollectionUpdatesViewModel : DotvvmViewModelBase
    {
        public List<Item> Items { get; set; } =
        [
            new Item() { Id = 1, Name = "Parent 1" },
            new Item() { Id = 2, Name = "Parent 2" },
        ];

        public int SelectedItem { get; set; }

        public List<Item> ChildItems { get; set; } = new();

        public List<int> SelectedChildItems { get; set; } = new();


        public void OnItemClick(Item item)
        {
            SelectedItem = item.Id;

            ChildItems = SelectedItem switch {
                1 => new List<Item>
                {
                        new Item { Id = 11, Name = "Child 1-1" },
                        new Item { Id = 12, Name = "Child 1-2" },
                    },
                2 => new List<Item>
                {
                        new Item { Id = 21, Name = "Child 2-1" },
                        new Item { Id = 22, Name = "Child 2-2" },
                        new Item { Id = 23, Name = "Child 2-3" },
                    },
                _ => throw new Exception()
            };
        }


        public class Item
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}

