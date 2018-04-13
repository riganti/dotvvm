using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ClientSideMethods
{
    public class ObjectOperationsViewModel : DotvvmViewModelBase
    {
        public PersonDto Person { get; set; } = new PersonDto {Age = 21, Name = "John doe"};

        [ClientSideMethod]
        public void UpdatePersonsAge()
        {
            Person.Age = 1;
        }

    }

    public class PersonDto
    {
        public string Name { get; set; }
        public int Age { get; set; }

    }
}
