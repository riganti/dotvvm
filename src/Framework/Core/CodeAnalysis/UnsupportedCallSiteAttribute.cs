using System;

namespace DotVVM.Core.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class UnsupportedCallSiteAttribute : Attribute
    {
        public readonly CallSiteType Type;

        public UnsupportedCallSiteAttribute(CallSiteType type)
        {
            Type = type;
        }
    }
}
