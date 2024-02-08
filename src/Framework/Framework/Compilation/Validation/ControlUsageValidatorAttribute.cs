using System;

namespace DotVVM.Framework.Compilation.Validation
{
    /// <summary>
    /// Call this static method for each compiled control of a matching type.
    /// The method should have the signature:
    /// <code> <![CDATA[
    /// public static IEnumerable< ControlUsageError> Validator(ResolvedControl control)
    /// ]]></code>.
    /// Optionally, an DotvvmConfiguration parameter may be present on the method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ControlUsageValidatorAttribute: Attribute
    {
        /// <summary> Ignore all validators from the base controls. </summary>
        public bool Override { get; set; }
        /// <summary>
        /// Call this method even on other controls when a property from the declaring class is used.
        /// The method will be called once per control.
        /// Properties on derived nor base classes do not trigger the validator.
        /// </summary>
        public bool IncludeAttachedProperties { get; set; }
    }
}
