using System;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class GridViewStaticCommandViewModel : DotvvmViewModelBase
    {
        public GridViewStaticCommandViewModel()
        {
            CustomersDataSet = new GridViewDataSet<CustomerData>
            {
                PagingOptions = new PagingOptions
                {
                    PageSize = 10
                }
            };
        }

        public GridViewDataSet<CustomerData> CustomersDataSet { get; set; }

        private static IQueryable<CustomerData> GetData()
        {
            return new[]
            {
                new CustomerData {CustomerId = 1, Name = "John Doe", BirthDate = DateTime.Parse("1976-04-01")},
                new CustomerData {CustomerId = 2, Name = "John Deer", BirthDate = DateTime.Parse("1984-03-02")},
                new CustomerData {CustomerId = 3, Name = "Johnny Walker", BirthDate = DateTime.Parse("1934-01-03")},
                new CustomerData {CustomerId = 4, Name = "Jim Hacker", BirthDate = DateTime.Parse("1912-11-04")},
                new CustomerData {CustomerId = 5, Name = "Joe E. Brown", BirthDate = DateTime.Parse("1947-09-05")}
            }.AsQueryable();
        }

        public override Task PreRender()
        {
            // fill dataset
            if (!Context.IsPostBack)
            {
                CustomersDataSet.LoadFromQueryable(GetData());
            }
            return base.PreRender();
        }

        [AllowStaticCommand]
        public void DeleteCustomerData(int customerId)
        {
            var customer = CustomersDataSet.Items.First(s => s != null && s.CustomerId == customerId);
            CustomersDataSet.Items.Remove(customer);
        }
    }
}