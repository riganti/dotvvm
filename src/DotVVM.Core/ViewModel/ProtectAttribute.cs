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
    public sealed class ProtectAttribute : Attribute
    {
        public ProtectMode Settings { get; private set; }

        public ProtectAttribute(ProtectMode settings)
        {
            this.Settings = settings;
        }
    }
}
