using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ControlMarkupOptionsAttribute : Attribute
    {


        public bool AllowContent { get; set; }

        public string DefaultContentProperty { get; set; }

    }
}