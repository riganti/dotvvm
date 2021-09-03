using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ViewModel
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ClientExtenderAttribute : Attribute
    {
        public string Name { get; set; }

        public int Order { get; set; }

        public object? Parameter { get; set; }

        public ClientExtenderAttribute(string name, object? parameter = null, int order = 0)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Parameter = parameter;
            this.Order = order;
        }
    }
}
