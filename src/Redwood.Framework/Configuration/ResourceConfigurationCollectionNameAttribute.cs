using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Configuration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ResourceConfigurationCollectionNameAttribute : Attribute
    {
        public string Name { get; set; }
        public ResourceConfigurationCollectionNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
