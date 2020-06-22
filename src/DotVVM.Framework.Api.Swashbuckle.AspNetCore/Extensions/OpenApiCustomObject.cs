using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.OpenApi.Any;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Extensions
{
    class OpenApiCustomObject : OpenApiPrimitive<object>
    {
        public override PrimitiveType PrimitiveType { get; } = PrimitiveType.Binary;

        public OpenApiCustomObject(object value)
            : base(value)
        {

        }
    }
}
