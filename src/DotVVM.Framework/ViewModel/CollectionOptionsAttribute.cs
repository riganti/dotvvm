using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.ViewModel
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CollectionOptionsAttribute : Attribute
    {

        public string KeyPropertyName { get; set; }

        
    }
}