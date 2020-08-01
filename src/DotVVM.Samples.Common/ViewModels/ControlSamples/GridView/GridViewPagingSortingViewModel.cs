using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class GridViewPagingSortingViewModel : DotvvmViewModelBase
    {
        private static IQueryable<CustomerData> GetData()
        {
            return new[]
            {
                new CustomerData() { CustomerId = 1, Name = "John Doe", BirthDate = DateTime.Parse("1976-04-01"), MessageReceived = false},
                new CustomerData() { CustomerId = 2, Name = "John Deer", BirthDate = DateTime.Parse("1984-03-02"), MessageReceived = false },
                new CustomerData() { CustomerId = 3, Name = "Johnny Walker", BirthDate = DateTime.Parse("1934-01-03"), MessageReceived = true},
                new CustomerData() { CustomerId = 4, Name = "Jim Hacker", BirthDate = DateTime.Parse("1912-11-04"), MessageReceived = true},
                new CustomerData() { CustomerId = 5, Name = "Joe E. Brown", BirthDate = DateTime.Parse("1947-09-05"), MessageReceived = false},
                new CustomerData() { CustomerId = 6, Name = "Jim Harris", BirthDate = DateTime.Parse("1956-07-06"), MessageReceived = false},
                new CustomerData() { CustomerId = 7, Name = "J. P. Morgan", BirthDate = DateTime.Parse("1969-05-07"), MessageReceived = false },
                new CustomerData() { CustomerId = 8, Name = "J. R. Ewing", BirthDate = DateTime.Parse("1987-03-08"), MessageReceived = false},
                new CustomerData() { CustomerId = 9, Name = "Jeremy Clarkson", BirthDate = DateTime.Parse("1994-04-09"), MessageReceived = false },
                new CustomerData() { CustomerId = 10, Name = "Jenny Green", BirthDate = DateTime.Parse("1947-02-10"), MessageReceived = false},
                new CustomerData() { CustomerId = 11, Name = "Joseph Blue", BirthDate = DateTime.Parse("1948-12-11"), MessageReceived = false},
                new CustomerData() { CustomerId = 12, Name = "Jack Daniels", BirthDate = DateTime.Parse("1968-10-12"), MessageReceived = true},
                new CustomerData() { CustomerId = 13, Name = "Jackie Chan", BirthDate = DateTime.Parse("1978-08-13"), MessageReceived = false},
                new CustomerData() { CustomerId = 14, Name = "Jasper", BirthDate = DateTime.Parse("1934-06-14"), MessageReceived = false},
                new CustomerData() { CustomerId = 15, Name = "Jumbo", BirthDate = DateTime.Parse("1965-06-15"), MessageReceived = false },
                new CustomerData() { CustomerId = 16, Name = "Junkie Doodle", BirthDate = DateTime.Parse("1977-05-16"), MessageReceived = false }
            }.AsQueryable();
        }

        public GridViewDataSet<CustomerData> CustomersDataSet { get; set; } = new GridViewDataSet<CustomerData>()
        {
            Pager =
            {
                PageSize = 10
            }
        };
        public GridViewDataSet<CustomerData> EmptyCustomersDataSet { get; set; } = new GridViewDataSet<CustomerData>()
        {
            Pager =
            {
                PageSize = 10
            }
        };

        public string SelectedSortColumn { get; set; }

        public List<CustomerData> Customers { get; set; }

        public List<CustomerData> Null { get; set; }

        public string CustomNameForName { get; set; } = "Name";

        public override Task PreRender()
        {
            // fill dataset
            CustomersDataSet.LoadFromQueryable(GetData());

            // fill customers
            if (SelectedSortColumn == "Name")
            {
                Customers = GetData().OrderBy(c => c.Name).ToList();
            }
            else if (SelectedSortColumn == "BirthDate")
            {
                Customers = GetData().OrderBy(c => c.BirthDate).ToList();
            }
            else
            {
                Customers = GetData().ToList();
            }

            return base.PreRender();
        }

        public void TestCommand()
        {
            CustomersDataSet.Sorter.ColumnSortClick(CustomersDataSet, "BirthDate");
        }

        public void SortCustomers(string column)
        {
            SelectedSortColumn = column;
        }

        [AllowStaticCommand]
        public List<CustomerData> GetDataList()
        {
            return GetData().ToList();
        }
    }

    public class CustomerData
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }

        public DateTime BirthDate { get; set; }
        public bool MessageReceived { get; set; }


        public void ChangeName()
        {
            Name = "xxx";
        }
    }
}
