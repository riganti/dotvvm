using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ComboBox
{
    public class ComboBoxDelaySyncViewModel : DotvvmViewModelBase
    {

        public int A { get; set; }

        public int Z { get; set; }

        public List<ComboBoxItem> Items { get; set; }



        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Items = new List<ComboBoxItem>()
                {
                    new ComboBoxItem() { Id = 1, Name = "One" },
                    new ComboBoxItem() { Id = 2, Name = "Two" },
                    new ComboBoxItem() { Id = 3, Name = "Three" }
                };
                A = 2;
                Z = 2;
            }
            return base.Init();
        }

        public void ChangeCollections()
        {
            Items = new List<ComboBoxItem>()
            {
                new ComboBoxItem() { Id = 4, Name = "Four" },
                new ComboBoxItem() { Id = 5, Name = "Five" },
                new ComboBoxItem() { Id = 6, Name = "Six" }
            };
            A = 5;
            Z = 5;
        }
    }

    public class ComboBoxItem
    {

        public int Id { get; set; }

        public string Name { get; set; }

    }
}

