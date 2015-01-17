using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ContainsRedwoodPropertiesAttribute : Attribute
    {
    }
}