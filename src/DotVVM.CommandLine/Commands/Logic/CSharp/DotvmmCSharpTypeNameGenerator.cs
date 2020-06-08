using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Core.Common;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag;

namespace DotVVM.CommandLine.Commands.Logic
{
    public class DotvmmCSharpTypeNameGenerator : DefaultTypeNameGenerator
    {
        private readonly CSharpGeneratorSettings settings;
        private readonly SwaggerDocument document;
        private Dictionary<string, string> pairs;

        public DotvmmCSharpTypeNameGenerator(CSharpGeneratorSettings settings, SwaggerDocument document)
        {
            this.settings = settings;
            this.document = document;
            GenerateTypeNamesPairs(document);
        }

        public override string Generate(JsonSchema4 schema, string typeNameHint, IEnumerable<string> reservedTypeNames)
        {
            if (pairs.TryGetValue(typeNameHint, out var type))
            {
                if (IsGenericType(type))
                {
                    type = GenerateGenericTypeName(type);
                }

                settings.ExcludedTypeNames = settings.ExcludedTypeNames.Concat(new[] { type }).ToArray();
                return type;
            }

            return base.Generate(schema, typeNameHint, reservedTypeNames);
        }

        private string GenerateGenericTypeName(string type)
        {
            // Its generic type, we need to resolve proper name of inner types
            var (genericTypeName, innerTypes) = RetrieveGenericParameters(type);
            var paramameters = new List<string>(innerTypes.Length);
            foreach (var innerType in innerTypes)
            {
                if (document.Definitions.TryGetValue(innerType, out var schema2))
                {
                    paramameters.Add(Generate(schema2, innerType, this.ReservedTypeNames));
                }
                else
                {
                    // Inner type is in FullName form
                    paramameters.Add(innerType);
                }
            }

            return CreateTypeName(genericTypeName, paramameters);
        }

        private void GenerateTypeNamesPairs(SwaggerDocument document)
        {
            settings.ExcludedTypeNames = (settings.ExcludedTypeNames ?? Array.Empty<string>());

            pairs = new Dictionary<string, string>();
            foreach (var definition in document.Definitions)
            {
                var extensionData = definition.Value.ExtensionData;
                if (extensionData != null && extensionData.TryGetValue(ApiConstants.DotvvmKnownTypeKey, out var type))
                {
                    pairs.Add(definition.Key, type.ToString());
                }
            }
        }

        private static bool IsGenericType(string typeName) => typeName.Contains('<');

        private static string CreateTypeName(string typeName, IEnumerable<string> parameters)
            => $"{typeName}<{string.Join(",", parameters)}>";

        private static (string TypeName, string[] Parameters) RetrieveGenericParameters(string type)
        {
            var index = type.IndexOf('<');
            var genericTypeName = type.Substring(0, index);
            var genericParams = type.Substring(index + 1, type.LastIndexOf('>') - index - 1);
            return (genericTypeName, genericParams.Split(','));
        }
    }
}
