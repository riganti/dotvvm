using System;

namespace DotVVM.Framework.Runtime.Filters
{
    /// <summary>
    /// Specifies that the class does not require authorization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NotAuthorizedAttribute : Attribute
    {
    }
}