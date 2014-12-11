using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.ViewModel
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class CryptoAttribute : Attribute
    {
        public CryptoSettings Settings { get; private set; }

        public CryptoAttribute(CryptoSettings settings)
        {
            this.Settings = settings;
        }

        // This is a named argument
        public int NamedInt { get; set; }
    }
}
