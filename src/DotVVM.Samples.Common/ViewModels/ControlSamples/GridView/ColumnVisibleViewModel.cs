using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class ColumnVisibleViewModel: DotvvmViewModelBase
    {
        private static IQueryable<CustomerData> GetData()
        {
            return new[]
            {
                new CustomerData() { CustomerId = 1, Name = "John Doe", BirthDate = DateTime.Parse("1976-04-01") },
                new CustomerData() { CustomerId = 2, Name = "John Deer", BirthDate = DateTime.Parse("1984-03-02") },
                new CustomerData() { CustomerId = 3, Name = "Johnny Walker", BirthDate = DateTime.Parse("1934-01-03") },
                new CustomerData() { CustomerId = 4, Name = "Jim Hacker", BirthDate = DateTime.Parse("1912-11-04") },
                new CustomerData() { CustomerId = 5, Name = "Joe E. Brown", BirthDate = DateTime.Parse("1947-09-05") },

            }.AsQueryable();
        }

        public ColumnVisibleViewModel()
        {
            CustomersDataSet = new GridViewDataSet<CustomerData>()
            {
                Pager =
                {
                    PageSize = 10
                }
            };
        }

        public GridViewDataSet<CustomerData> CustomersDataSet { get; set; }
        public bool IsBirthDateVisible { get; set; } = true;

        public override Task PreRender()
        {
            // fill dataset
            if (!Context.IsPostBack)
            {
                CustomersDataSet.LoadFromQueryable(GetData());
            }
            return base.PreRender();
        }
    }
}
