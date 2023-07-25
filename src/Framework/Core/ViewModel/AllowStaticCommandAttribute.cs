using System;

namespace DotVVM.Framework.ViewModel
{
    /// <summary> Allows DotVVM to call the method from staticCommand. </summary>
    /// <remarks>
    /// This attribute must be used to prevent attackers from calling any method in your system.
    /// While DotVVM signs the method names used staticCommand and it shouldn't be possible to execute any other method,
    /// the attribute offers a decent protection against RCE in case the Asp.Net Core encryption keys are compromised. </remarks>
    public class AllowStaticCommandAttribute : Attribute
    {
        public StaticCommandValidation Validation { get; }

        public AllowStaticCommandAttribute() : this(StaticCommandValidation.None) { }

        public AllowStaticCommandAttribute(StaticCommandValidation validation)
        {
            Validation = validation;
        }
    }
}
