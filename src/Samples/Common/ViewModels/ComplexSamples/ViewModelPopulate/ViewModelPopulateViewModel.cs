using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ComplexSamples.ViewModelPopulate
{
    public class ViewModelPopulateViewModel : DotvvmViewModelBase
    {

        public NonDeserializableObject NonDeserializableObject { get; set; } = new(1, "");


        public void DoSomething()
        {
            NonDeserializableObject.Test = NonDeserializableObject.Test + "success";
        }
    }

    public class NonDeserializableObject
    {

        public string Test { get; set; }

        public NonDeserializableObject(int nonPropertyField, string test)
        {
            
        }
    }
}

