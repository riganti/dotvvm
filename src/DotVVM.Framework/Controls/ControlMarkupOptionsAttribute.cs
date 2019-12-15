using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ControlMarkupOptionsAttribute : Attribute
    {


        public bool AllowContent { get; set; } = true;

        public string DefaultContentProperty { get; set; }

    }
}