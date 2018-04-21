using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ClientSideMethods
{
    public class ObjectOperationsViewModel : MasterPageViewModel
    {
        public PersonDto Person { get; set; } = new PersonDto();

        [ClientSideMethod]
        public void UpdatePersonsAge()
        {
            Person.Age = 1;
        }

        [ClientSideMethod]
        public void CreateNewPerson()
        {
            Person = new PersonDto("Karel", 27);
        }
    }

    public class PersonDto
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public PersonDto() { }

        [ClientSideConstructor]
        public PersonDto(string name, int age)
        {
            Name = name;
            Age = age;
        }

    }
}
