using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ViewModel
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ClientExtenderAttribute : Attribute
    {
        public string Name { get; set; }

        public ClientExtenderAttribute(string name)
        {
            this.Name = name;
        }
    }
}
