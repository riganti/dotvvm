using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace fallback_test_project.ViewModels
{
    public class DefaultViewModel : DotvvmViewModelBase
    {
        
        public string Title { get; set; }


        public DefaultViewModel()
        {
            Title = "Hello from DotVVM!";
        }

    }
}
