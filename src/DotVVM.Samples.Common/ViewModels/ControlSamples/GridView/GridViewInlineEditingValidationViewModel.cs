using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class GridViewInlineEditingValidationViewModel : DotvvmViewModelBase
    {

        private static IQueryable<CustomerDataValidation> GetDataValidation()
        {
            return new[]
            {
                new CustomerDataValidation() { CustomerId = 1, Name = "John Doe", BirthDate = DateTime.Parse("1976-04-01"), Email= "test@test.cz", Money = 120011 },
                new CustomerDataValidation() { CustomerId = 2, Name = "John Deer", BirthDate = DateTime.Parse("1984-03-02"), Email= "test@test.cz", Money = 1550011 },
                new CustomerDataValidation() { CustomerId = 3, Name = "Johnny Walker", BirthDate = DateTime.Parse("1934-01-03"), Email= "test@test.cz", Money = 210000},
            }.AsQueryable();
        }


        public GridViewDataSet<CustomerDataValidation> CustomersDataSet { get; set; }

        public List<CustomerDataValidation> Customers { get; set; }

        public List<CustomerDataValidation> Null { get; set; }

        public bool EditMode { get; set; } = true;

        public int EditRowId { get; set; } = -1;

        public GridViewInlineEditingValidationViewModel()
        {
            CustomersDataSet = new GridViewDataSet<CustomerDataValidation>()
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
            CustomersDataSet.LoadFromQueryable(GetDataValidation());
            return base.PreRender();
        }

        public void EditItem(CustomerDataValidation item)
        {
            EditRowId = item.CustomerId;
            CustomersDataSet.RowEditOptions.EditRowId = item.CustomerId;
            EditMode = !EditMode;
        }

        public void UpdateItem(CustomerDataValidation item)
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
    }

    public class CustomerDataValidation
    {
        public int CustomerId { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTime BirthDate { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public int Money { get; set; }
    }
}
