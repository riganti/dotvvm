using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Controls;
using DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.AppendableDataPager
{
    public class AppendableDataPagerViewModel : DotvvmViewModelBase
    {
        public GridViewDataSet<CustomerData> Customers { get; set; } = new() {
            PagingOptions = new PagingOptions {
                PageSize = 3
            }
        };

        private static IQueryable<CustomerData> GetData()
        {
            return new[]
            {
                new CustomerData {CustomerId = 1, Name = "John Doe", BirthDate = DateTime.Parse("1976-04-01")},
                new CustomerData {CustomerId = 2, Name = "John Deer", BirthDate = DateTime.Parse("1984-03-02")},
                new CustomerData {CustomerId = 3, Name = "Johnny Walker", BirthDate = DateTime.Parse("1934-01-03")},
                new CustomerData {CustomerId = 4, Name = "Jim Hacker", BirthDate = DateTime.Parse("1912-11-04")},
                new CustomerData {CustomerId = 5, Name = "Joe E. Brown", BirthDate = DateTime.Parse("1947-09-05")},
                new CustomerData {CustomerId = 6, Name = "Jack Daniels", BirthDate = DateTime.Parse("1956-07-06")},
                new CustomerData {CustomerId = 7, Name = "James Bond", BirthDate = DateTime.Parse("1965-05-07")},
                new CustomerData {CustomerId = 8, Name = "John Smith", BirthDate = DateTime.Parse("1974-03-08")},
                new CustomerData {CustomerId = 9, Name = "Jack & Jones", BirthDate = DateTime.Parse("1976-03-22")},
                new CustomerData {CustomerId = 10, Name = "Jim Bill", BirthDate = DateTime.Parse("1974-09-20")},
                new CustomerData {CustomerId = 11, Name = "James Joyce", BirthDate = DateTime.Parse("1982-11-28")},
                new CustomerData {CustomerId = 12, Name = "Joudy Jane", BirthDate = DateTime.Parse("1958-12-14")}
            }.AsQueryable();
        }
        public override Task PreRender()
        {
            // fill dataset
            if (!Context.IsPostBack)
            {
                Customers.LoadFromQueryable(GetData());
            }
            return base.PreRender();
        }

        [AllowStaticCommand]
        public static async Task<GridViewDataSet<CustomerData>> LoadNextPage(GridViewDataSetOptions options)
        {
            await Task.Delay(2000);
            var dataSet = new GridViewDataSet<CustomerData>();
            dataSet.ApplyOptions(options);
            dataSet.LoadFromQueryable(GetData());
            return dataSet;
        }
    }
}

