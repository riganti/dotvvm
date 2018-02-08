using System;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public static class SwashbuckleApiHelpers
    {
        public static bool IsKnownType(this DotvvmApiKnownTypesOptions options, Type type)
        {
            var typeToCompare = type;
            if (type.IsGenericType)
            {
                if (options.KnownTypes.Contains(typeToCompare))
                {
                    return true;
                }
                typeToCompare = type.GetGenericTypeDefinition();
            }

            return options.KnownTypes.Contains(typeToCompare);
        }
    }
}
