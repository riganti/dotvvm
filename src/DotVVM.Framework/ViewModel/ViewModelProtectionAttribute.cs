using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ViewModel
{
    /// <summary>
    /// Defines the protection mode of the property in ViewModel.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ViewModelProtectionAttribute : Attribute
    {
        public ViewModelProtectionSettings Settings { get; private set; }

        public ViewModelProtectionAttribute(ViewModelProtectionSettings settings)
        {
            this.Settings = settings;
        }
    }
}
