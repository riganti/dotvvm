using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class GridViewInlineEditingViewModel : DotvvmViewModelBase
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

        public GridViewDataSet<CustomerData> CustomersDataSet { get; set; }

        public string SelectedSortColumn { get; set; }

        public List<CustomerData> Customers { get; set; }

        public List<CustomerData> Null { get; set; }

        public bool EditMode { get; set; } = true;

        public int EditRowId { get; set; } = 1;

        public bool IsBirthDateVisible { get; set; } = true;

        public GridViewInlineEditingViewModel()
        {
            CustomersDataSet = new GridViewDataSet<CustomerData>()
            {
                Pager =
                {
                    PageSize = 10
                },
                RowEditOptions =
                {
                    PrimaryKeyPropertyName = "CustomerId",
                    EditRowId = EditRowId 
                }
            };
        }

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

        public void EditItem(CustomerData item)
        {
            EditRowId = item.CustomerId;
            CustomersDataSet.RowEditOptions.EditRowId = item.CustomerId;
            EditMode = !EditMode;
        }

        public void UpdateItem(CustomerData item)
        {
            //save item
            EditRowId = -1;
            CustomersDataSet.RowEditOptions.EditRowId = -1;
            EditMode = !EditMode;
        }

        public void CancelEditItem()
        {
            EditRowId = -1;
            CustomersDataSet.RowEditOptions.EditRowId = -1;
            EditMode = !EditMode;
        }

        public void SortCustomers(string column)
        {
            SelectedSortColumn = column;
        }
    }


}
