using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class GridViewInlineEditingPrimaryKeyStringViewModel : DotvvmViewModelBase
    {
        private static IQueryable<CustomerDataString> GetDataString()
        {
            return new[]
            {
                new CustomerDataString() { CustomerId = "A", Name = "John Doe", BirthDate = DateTime.Parse("1976-04-01"), Email= "test@test.cz" },
                new CustomerDataString() { CustomerId = "B", Name = "John Deer", BirthDate = DateTime.Parse("1984-03-02"), Email= "test@test.cz" },
                new CustomerDataString() { CustomerId = "C", Name = "Johnny Walker", BirthDate = DateTime.Parse("1934-01-03"), Email= "test@test.cz" },

            }.AsQueryable();
        }


        public GridViewDataSet<CustomerDataString> CustomersDataSet { get; set; }



        public List<CustomerDataGuid> Customers { get; set; }

        public List<CustomerDataGuid> Null { get; set; }

        public bool EditMode { get; set; } = true;

        public string EditRowId { get; set; } = "Z";

        public bool FirstLoad { get; set; } = true;

        public GridViewInlineEditingPrimaryKeyStringViewModel()
        {
            CustomersDataSet = new GridViewDataSet<CustomerDataString>()
            {
                PagingOptions = new PagingOptions()
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
            if(FirstLoad)
            {
                CustomersDataSet.LoadFromQueryable(GetDataString());
                FirstLoad = false;
            }
            
            return base.PreRender();
        }

        public void EditItem(CustomerDataString item)
        {
            EditRowId = item.CustomerId;
            CustomersDataSet.RowEditOptions.EditRowId = item.CustomerId;
            EditMode = !EditMode;
        }

        public void UpdateItem(CustomerDataString item)
        {
            //save item
            EditRowId = "Z";
            CustomersDataSet.RowEditOptions.EditRowId = EditRowId;

            var updateItem = CustomersDataSet.Items.FirstOrDefault(s => s.CustomerId == item.CustomerId);
            updateItem = item;
            EditMode = !EditMode;
        }

        public void CancelEditItem()
        {
            EditRowId ="Z";
            CustomersDataSet.RowEditOptions.EditRowId = EditRowId;
            EditMode = !EditMode;
        }
    }

    public class CustomerDataString
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }

        public DateTime BirthDate { get; set; }
        public string Email { get; internal set; }
    }
}
