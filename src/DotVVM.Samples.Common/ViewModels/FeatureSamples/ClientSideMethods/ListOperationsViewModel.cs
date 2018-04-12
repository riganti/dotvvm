using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ClientSideMethods
{
    public class ListOperationsViewModel : DotvvmViewModelBase
    {
        public List<string> NamesList { get; set; } = new List<string> { "test", "test1" };

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
    }
}
