using System;
using System.ComponentModel;

namespace DotVVM.Framework.ViewModel
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyOptionsAttribute : Attribute
    {

        public BindingDirection BindingDirection { get; set; }

    }
}