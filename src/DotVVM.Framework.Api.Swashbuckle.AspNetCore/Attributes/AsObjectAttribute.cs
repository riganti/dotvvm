using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Api.Swashbuckle.Attributes
{
    /// <summary>
    /// Configure Swagger generator not to generate the method with the properties of the complex-type parameter passed as separate arguments, but to generate a method which accepts one argument of the specified complex type.
    /// This attribute is used together with the FromQuery attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class AsObjectAttribute : Attribute
    {
        public Type ClientType { get; set; }

        public AsObjectAttribute()
        {
        }

        public AsObjectAttribute(Type clientType)
        {
            ClientType = clientType;
        }
    }
}
