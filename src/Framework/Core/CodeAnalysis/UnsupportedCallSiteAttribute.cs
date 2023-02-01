using System;

namespace DotVVM.Framework.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class UnsupportedCallSiteAttribute : Attribute
    {
        public readonly CallSiteType Type;
        public readonly string? Reason;

        public UnsupportedCallSiteAttribute(CallSiteType type, string? reason = null)
        {
            Type = type;
            Reason = reason;
        }
    }
}
