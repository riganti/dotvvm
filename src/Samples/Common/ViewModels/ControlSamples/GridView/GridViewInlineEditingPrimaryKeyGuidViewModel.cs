using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Controls;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class GridViewInlineEditingPrimaryKeyGuidViewModel : DotvvmViewModelBase
    {

        private static IQueryable<CustomerDataGuid> GetDataGuid()
        {
            return new[]
            {
                new CustomerDataGuid() { CustomerId = Guid.Parse("9536d712-2e91-43d2-8ebb-93fbec31cf34"), Name = "John Doe", BirthDate = DateTime.Parse("1976-04-01"), Email= "test@test.cz" },
                new CustomerDataGuid() { CustomerId = Guid.Parse("090dd6fd-fb85-42f0-b1d2-c510130a8073"), Name = "John Deer", BirthDate = DateTime.Parse("1984-03-02"), Email= "test@test.cz" },
                new CustomerDataGuid() { CustomerId = Guid.Parse("11a23c2e-1e56-4cec-accb-9e6d9bb1f20b"), Name = "Johnny Walker", BirthDate = DateTime.Parse("1934-01-03"), Email= "test@test.cz" },

            }.AsQueryable();
        }


        public GridViewDataSet<CustomerDataGuid> CustomersDataSet { get; set; }



        public List<CustomerDataGuid> Customers { get; set; }

        public List<CustomerDataGuid> Null { get; set; }

        public bool EditMode { get; set; } = true;

        public Guid EditRowId { get; set; } = Guid.NewGuid();

        public bool FirstLoad { get; set; } = true;

        public GridViewInlineEditingPrimaryKeyGuidViewModel()
        {
            CustomersDataSet = new GridViewDataSet<CustomerDataGuid>() {
                PagingOptions = 
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
            if (FirstLoad)
            {
                CustomersDataSet.LoadFromQueryable(GetDataGuid());
                FirstLoad = false;
            }

            return base.PreRender();
        }

        public void EditItem(CustomerDataGuid item)
        {
            EditRowId = item.CustomerId;
            CustomersDataSet.RowEditOptions.EditRowId = item.CustomerId;
            EditMode = !EditMode;
        }

        public void UpdateItem(CustomerDataGuid item)
        {
            //save item
            EditRowId = Guid.NewGuid();
            CustomersDataSet.RowEditOptions.EditRowId = EditRowId;
            var updateItem = CustomersDataSet.Items.FirstOrDefault(s => s.CustomerId == item.CustomerId);
            updateItem = item;
            EditMode = !EditMode;
        }

        public void CancelEditItem()
        {
            EditRowId = Guid.NewGuid();
            CustomersDataSet.RowEditOptions.EditRowId = EditRowId;
            EditMode = !EditMode;
        }
    }

    public class CustomerDataGuid
    {
        public Guid CustomerId { get; set; }
        public string Name { get; set; }

        public DateTime BirthDate { get; set; }
        public string Email { get; internal set; }
    }
}
