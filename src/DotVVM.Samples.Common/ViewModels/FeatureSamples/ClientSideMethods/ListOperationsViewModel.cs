using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ClientSideMethods
{
    public class ListOperationsViewModel : DotvvmViewModelBase
    {
        public List<string> NamesList { get; set; } = new List<string> { "test", "test1" };

        public int Index { get; set; } = 0;

        [ClientSideMethod]
        public void Add()
        {
            NamesList.Add("test" + Index);
            Index++;
        }

        [ClientSideMethod]
        public void RemoveTest()
        {
            Remove("test");
        }

        [ClientSideMethod]
        public void Remove(string name)
        {
            NamesList.Remove(name);
        }

        [ClientSideMethod]
        public void Clear()
        {
            NamesList.Clear();
        }

        [ClientSideMethod]
        public void Iterate()
        {
            for (int i = 0; i < NamesList.Count; i++)
            {
                var name = NamesList[i];
                if (!name.Contains("iterated"))
                    NamesList[i] = name + " iterated";
            }
        }

        public void SaveViaPostback()
        {
            for (var i = 0; i < NamesList.Count; i++)
            {
                NamesList[i] += " saved";
            }
        }
    }
}
