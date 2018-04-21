using System;

namespace DotVVM.Framework.ViewModel
{
    /// <summary>
    /// Specifies whether constructor should be translated to Javascript and send to client in order to create the object in JavaScript.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class ClientSideConstructorAttribute : Attribute
    {
        
    }
}
