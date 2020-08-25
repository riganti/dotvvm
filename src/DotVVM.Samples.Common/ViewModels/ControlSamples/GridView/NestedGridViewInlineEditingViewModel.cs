using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using Newtonsoft.Json;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public GridViewDataSet<ShoppingCartItem> ShoppingCartItems { get; set; }
    }

    public class ShoppingCartItem
    {
        public string Item { get; set; }
        public int Quantity { get; set; }
    }

    public class NestedGridViewInlineEditingViewModel : DotvvmViewModelBase
    {
        public GridViewDataSet<Customer> Customers { get; set; } = new GridViewDataSet<Customer>() {
            RowEditOptions = { PrimaryKeyPropertyName = "Id" }
        };

        private static IQueryable<Customer> GetCustomersData()
        {
            var customers = new List<Customer>()
            {
                new Customer() { Id = 1, Name = "John Doe" },
                new Customer() { Id = 2, Name = "John Deer" },
                new Customer() { Id = 3, Name = "Johnny Walker" },
                new Customer() { Id = 4, Name = "Jim Hacker" },
                new Customer() { Id = 5, Name = "Joe E. Brown" },
            };
            customers.ForEach(customer => {
                customer.ShoppingCartItems = new GridViewDataSet<ShoppingCartItem>() {
                    RowEditOptions = { PrimaryKeyPropertyName = "Item" }
                };
                customer.ShoppingCartItems.LoadFromQueryable(GetShoppingCartData());
            });

            return customers.AsQueryable();
        }

        private static IQueryable<ShoppingCartItem> GetShoppingCartData()
        {
            var shoppingCartItems = new List<ShoppingCartItem>()
            {
                new ShoppingCartItem() { Item = "Apple", Quantity = 3 },
                new ShoppingCartItem() { Item = "Orange", Quantity = 11 },
            };

            return shoppingCartItems.AsQueryable();
        }

        public override async Task PreRender()
        {
            if (!Context.IsPostBack)
            {
                Customers.LoadFromQueryable(GetCustomersData());
            }
            await base.PreRender();
        }

        public void EditShoppingCart(Customer customer, ShoppingCartItem item)
        {
            //var student = Students.Items.Where(x => x.Grades.Items.Any(y => y.GradeId == grade.GradeId)).First();
            customer.ShoppingCartItems.RowEditOptions.EditRowId = item.Item;
        }

        public void UpdateShoppingCart(Customer customer, ShoppingCartItem item)
        {
            customer.ShoppingCartItems.RowEditOptions.EditRowId = null;
        }

        public void CancelEditShoppingCart()
        {
            Customers.RequestRefresh();
        }

        public void EditCustomer(Customer customer)
        {
            Customers.RowEditOptions.EditRowId = customer.Id;
        }

        public void UpdateCustomer(Customer customer)
        {
            Customers.RowEditOptions.EditRowId = null;
        }

        public void CancelEditCustomer()
        {
            Customers.RowEditOptions.EditRowId = null;
            Customers.RequestRefresh();
        }
    }
}
