using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Serialization
{
    public class DeserializationVirtualElementsViewModel : DotvvmViewModelBase
    {

        public ChildObject ObjectInVirtualElement { get; set; } = new ChildObject();


        public override Task Load()
        {
            if (!Context.IsPostBack)
            {
                ObjectInVirtualElement.Entries.Add(new ChildEntry() { Value = "One" });
                ObjectInVirtualElement.Entries.Add(new ChildEntry() { Value = "Two" });
                ObjectInVirtualElement.Entries.Add(new ChildEntry() { Value = "Three" });
            }

            return base.Load();
        }


        public class ChildObject
        {

            public List<ChildEntry> Entries { get; set; } = new List<ChildEntry>();

            public ChildEntry NewEntry { get; set; } = new ChildEntry();


            public void Add()
            {
                Entries.Add(NewEntry);
                NewEntry = new ChildEntry();
            }

            public void Remove(ChildEntry entry)
            {
                Entries.Remove(entry);
            }
        }

        public class ChildEntry
        {
            public string Value { get; set; }
        }
    }
}

