using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ClientSideMethods
{
    public class ObjectOperationsViewModel : MasterpageViewModel
    {
        public PersonDto Person { get; set; } = new PersonDto();

        public string Name { get; set; }
        public int Age { get; set; }

        public List<PersonDto> Persons { get; set; } = new List<PersonDto>();

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

        [ClientSideMethod]
        public void AddPerson(string name, int age)
        {
            Persons.Add(new PersonDto(Name, Age));
        }

        [ClientSideMethod]
        public void RemovePerson(PersonDto dto)
        {
            Persons.Remove(dto);
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
