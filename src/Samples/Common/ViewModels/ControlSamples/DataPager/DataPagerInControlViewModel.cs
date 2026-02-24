using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.DataPager
{
    public class DataPagerInControlViewModel : DotvvmViewModelBase
    {
        public GridViewDataSet<Person> People { get; set; } = new GridViewDataSet<Person>() {
            PagingOptions = new PagingOptions() {
                PageSize = 5
            }
        };

        public override Task PreRender()
        {
            if (People.IsRefreshRequired)
            {
                var query = GetPeopleQuery();
                People.LoadFromQueryable(query);
            }
            return base.PreRender();
        }

        private IQueryable<Person> GetPeopleQuery()
        {
            // Sample data - 20 people
            var people = Enumerable.Range(1, 20)
                .Select(i => new Person {
                    Id = i,
                    Name = $"Person {i}",
                    Email = $"person{i}@example.com",
                    Age = 20 + i
                })
                .AsQueryable();

            return people;
        }
        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public int Age { get; set; }
        }
    }
}

