using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using System.Diagnostics;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample41ViewModel : DotvvmViewModelBase
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
                new CustomerData() { CustomerId = 6, Name = "Jim Harris", BirthDate = DateTime.Parse("1956-07-06") },
                new CustomerData() { CustomerId = 7, Name = "J. P. Morgan", BirthDate = DateTime.Parse("1969-05-07") },
                new CustomerData() { CustomerId = 8, Name = "J. R. Ewing", BirthDate = DateTime.Parse("1987-03-08") },
                new CustomerData() { CustomerId = 9, Name = "Jeremy Clarkson", BirthDate = DateTime.Parse("1994-04-09") },
                new CustomerData() { CustomerId = 10, Name = "Jenny Green", BirthDate = DateTime.Parse("1947-02-10") },
                new CustomerData() { CustomerId = 11, Name = "Joseph Blue", BirthDate = DateTime.Parse("1948-12-11") },
                new CustomerData() { CustomerId = 12, Name = "Jack Daniels", BirthDate = DateTime.Parse("1968-10-12") },
                new CustomerData() { CustomerId = 13, Name = "Jackie Chan", BirthDate = DateTime.Parse("1978-08-13") },
                new CustomerData() { CustomerId = 14, Name = "Jasper", BirthDate = DateTime.Parse("1934-06-14") },
                new CustomerData() { CustomerId = 15, Name = "Jumbo", BirthDate = DateTime.Parse("1965-06-15") },
                new CustomerData() { CustomerId = 16, Name = "Junkie Doodle", BirthDate = DateTime.Parse("1977-05-16") }
            }.AsQueryable();
        }

        [Bind(Direction.ClientToServerInPostbackPath | Direction.ServerToClient)]
        public GridViewDataSet<CustomerData> CustomersDataSet { get; set; }

        public Sample41ViewModel()
        {
            CustomersDataSet = new GridViewDataSet<CustomerData>() { PageSize = 10 };
        }

        public override Task PreRender()
        {
            // fill dataset
            CustomersDataSet.LoadFromQueryable(GetData());

            return base.PreRender();
        }

        public void Action(int customerId)
        {
            var customer = CustomersDataSet.Items.First(s => s != null && s.CustomerId == customerId);
            Debug.Write($"Action performed on customer { customer.Name }");
        }
    }
}