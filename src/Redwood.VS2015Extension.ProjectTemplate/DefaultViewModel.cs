using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.ViewModel;

namespace $safeprojectname$.ViewModels
{
    public class DefaultViewModel : RedwoodViewModelBase
    {
        
        public string Title { get; set; }


        public DefaultViewModel()
        {
            Title = "Hello from Redwood!";
        }

    }
}
