using System;
using System.Linq;
using DotVVM.Core.Common;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public class HandleKnownTypesDocumentFilter : IDocumentFilter
    {
        private readonly IOptions<DotvvmApiKnownTypesOptions> knownTypesOptions;

        public HandleKnownTypesDocumentFilter(IOptions<DotvvmApiKnownTypesOptions> knownTypesOptions)
        {
            this.knownTypesOptions = knownTypesOptions;
        }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            var knownTypes = knownTypesOptions.Value;
            foreach (var definition in swaggerDoc.Definitions)
            {
                if (definition.Value.Extensions.TryGetValue(ApiConstants.DotvvmTypeKey, out var objType) && objType is Type underlayingType
                    && knownTypes.IsKnownType(underlayingType))
                {
                    var name = CreateProperName(underlayingType, swaggerDoc);
                    definition.Value.Extensions.Add(ApiConstants.DotvvmKnownTypeKey, name);
                }
            }

            foreach (var definition in swaggerDoc.Definitions)
            {
                definition.Value.Extensions.Remove(ApiConstants.DotvvmTypeKey);
            }
        }

        public string CreateProperName(Type type, SwaggerDocument swaggerDoc)
        {
            if (type.GetGenericArguments().Length == 0)
            {
                return CreateNameWithNamespace(type);
            }

            var genericArguments = type.GetGenericArguments().Select(t => CreateNameForGenericParameter(t, swaggerDoc));
            var unmangledName = GetNameWithoutGenericArity(type);

            return type.Namespace + '.' + unmangledName + '<' + string.Join(",", genericArguments) + '>';
        }

        public string CreateNameForGenericParameter(Type type, SwaggerDocument swaggerDoc)
        {
            var definition = swaggerDoc.Definitions
                .Where(d => d.Value.Extensions.TryGetValue(ApiConstants.DotvvmTypeKey, out var objType) && (Type)objType == type)
                .FirstOrDefault();

            return definition.Key ?? type.FullName;
        }

        public static string GetNameWithoutGenericArity(Type type) => type.Name.Substring(0, type.Name.IndexOf('`'));

        private static string CreateNameWithNamespace(Type type) => type.Namespace + '.' + type.Name;
    }
}
