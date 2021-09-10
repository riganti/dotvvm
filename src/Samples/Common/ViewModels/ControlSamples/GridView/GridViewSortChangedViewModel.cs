﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.GridView
{
    public class GridViewSortChangedViewModel : DotvvmViewModelBase
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
            PagingOptions = new PagingOptions()
            {
                PageSize = 10
            },
            SortingOptions = new SortingOptions()
            {
                SortExpression = nameof(CustomerData.CustomerId)
            }
        };

        public override Task PreRender()
        {
            if (CustomersDataSet.IsRefreshRequired)
            {
                CustomersDataSet.LoadFromQueryable(GetData());
            }

            return base.PreRender();
        }

        public void CustomSort(string column)
        {
            CustomersDataSet.SortingOptions.SortExpression = column;
            CustomersDataSet.SortingOptions.SortDescending = false;
        }
    }
}
