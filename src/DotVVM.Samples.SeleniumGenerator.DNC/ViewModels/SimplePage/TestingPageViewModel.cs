using System;
using System.Collections.Generic;
using System.Threading;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace SampleApp1.ViewModels.SimplePage
{
    public class TestingPageViewModel : DotvvmViewModelBase
    {
        public string Title { get; set; }

        public int Result { get; set; }

        public TestingPageViewModel()
        {
            Title = "Hello from DotVVM!";
            Files = new UploadedFilesCollection();
        }

        public void CalculateIt()
        {
            // the calculation is very complicated and it takes a lot of time to get the result
            Thread.Sleep(5000);

            var random = new Random(DateTime.Now.Millisecond);
            Result = random.Next();
        }

        public CustomerDTO SelectedCustomerListBox { get; set; }

        public UploadedFilesCollection Files { get; set; }

        public GridViewDataSet<CustomerDTO> Customers { get; set; } = new GridViewDataSet<CustomerDTO>
        {
            Items = new List<CustomerDTO>
            {
                new CustomerDTO{ Id = 1, Name = "Ahoj", IsChecked = true},
                new CustomerDTO{ Id = 2, Name = "Cus"},
                new CustomerDTO{ Id = 3, Name = "Zdar"},
                new CustomerDTO{ Id = 4, Name = "Polc", IsChecked = true},
                new CustomerDTO{ Id = 5, Name = "Popo"},
                new CustomerDTO{ Id = 6, Name = "CCC"},
                new CustomerDTO{ Id = 7, Name = "Zdar"},
                new CustomerDTO{ Id = 8, Name = "CACa", IsChecked = true},
            },
            PagingOptions = { PageSize = 3, TotalItemsCount = 8 }

        };
        public int? SelectedCustomer { get; set; }
    }

    public class CustomerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PostalCode { get; set; }

        public bool IsChecked { get; set; }
        public string TelNumber { get; set; }
    }
}
