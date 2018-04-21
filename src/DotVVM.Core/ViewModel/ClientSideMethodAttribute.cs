using System;

namespace DotVVM.Framework.ViewModel
{
    /// <summary>
    /// Specifies if function should be translated to Javascript and send to client.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ClientSideMethodAttribute : Attribute
    {
    }
}
