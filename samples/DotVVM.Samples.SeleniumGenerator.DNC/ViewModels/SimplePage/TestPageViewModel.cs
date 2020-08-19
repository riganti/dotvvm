using System.Collections.Generic;
using DotVVM.Framework.ViewModel;
using System;

namespace SampleApp1.ViewModels.SimplePage
{
    public class TestPageViewModel : DotvvmViewModelBase
    {
        public TestPageViewModel()
        {
            Users = new List<UserDTO>
            {
                new UserDTO
                {
                    Name = "Adam"
                },
                new UserDTO
                {
                    Name = "Patrik"
                }
            };
        }

        public List<UserDTO> Users { get; set; }

        public void OnRefreshClicked()
        {
            return;
        }
    }

    public class UserDTO
    {
        public string Name { get; set; }
    }
}

