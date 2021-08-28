using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Serialization
{
    public class ObservableCollectionShouldContainObservablesViewModel : DotvvmViewModelBase
    {

        public IList<ObservableCollectionItem> Items => new[]
        {
            new ObservableCollectionItem() { Id = 1, Name = "One" },
            new ObservableCollectionItem() { Id = 2, Name = "Two" },
            new ObservableCollectionItem() { Id = 3, Name = "Three" }
        };

        public List<int> SelectedItemIds { get; set; } = new List<int>();

        public string Result { get; set; }

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                SelectedItemIds.Add(1);
                SelectedItemIds.Add(2);
                SelectedItemIds.Add(3);
            }

            return base.Init();
        }


        public void PrintItems()
        {
            Result = string.Join(",", SelectedItemIds);
        }

    }

    public class ObservableCollectionItem
    {

        public string Name { get; set; }

        public int Id { get; set; }

    }
}