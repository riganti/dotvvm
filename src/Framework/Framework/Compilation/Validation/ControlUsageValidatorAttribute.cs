using System;

namespace DotVVM.Framework.Compilation.Validation
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ControlUsageValidatorAttribute: Attribute
    {
        public bool Override { get; set; }
    }
}
