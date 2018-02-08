using System;
using System.Linq;
using DotVVM.Core.Common;
using DotVVM.Framework.Controls;
using Microsoft.Extensions.Options;
using Swashbuckle.Swagger;

namespace DotVVM.Framework.Api.Swashbuckle.Owin.Filters
{
    public class HandleKnownTypesSchemaFilter : ISchemaFilter
    {
        private Type[] knownTypes;

        public HandleKnownTypesSchemaFilter()
        {
            knownTypes = new[] { typeof(GridViewDataSet<>), typeof(IPagingOptions), typeof(ISortingOptions), typeof(IRowEditOptions) };
        }

        //public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        //{
        //    HandleReturnType(operation, apiDescription.ActionDescriptor);

        //    HandleParameterTypes(operation, apiDescription.ActionDescriptor);
        //}

        //private void HandleParameterTypes(Operation operation, HttpActionDescriptor actionDescriptor)
        //{
        //    var knownTypes = actionDescriptor.GetParameters()
        //                    .Select(descriptor => descriptor.ParameterType)
        //                    .Where(IsKnownType)
        //                    .Select(parameterType => parameterType.FullName)
        //                    .Distinct();

        //    if (knownTypes.Any())
        //    {
        //        operation.vendorExtensions.Add("x-knownType", string.Join(";", knownTypes));
        //    }
        //}

        //private void HandleReturnType(Operation operation, HttpActionDescriptor actionDescriptor)
        //{
        //    var returnType = actionDescriptor.ReturnType;
        //    if (IsKnownType(returnType))
        //    {
        //        operation.vendorExtensions.Add("x-returns-knownType", returnType.Namespace + '.' + PrettyName(returnType));
        //    }
        //}

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (IsKnownType(type))
            {
                schema.vendorExtensions.Add(ApiConstants.DotvvmKnownTypeKey, type.Namespace + '.' + PrettyName(type));
            }
        }

        private bool IsKnownType(Type type)
        {
            var typeToCompare = type;
            if (type.IsGenericType)
            {
                typeToCompare = type.GetGenericTypeDefinition();
            }

            return knownTypes.Contains(typeToCompare);
        }

        private static string PrettyName(Type type)
        {
            if (type.GetGenericArguments().Length == 0)
            {
                return type.Name;
            }
            var genericArguments = type.GetGenericArguments();
            var typeDefeninition = type.Name;
            var unmangledName = typeDefeninition.Substring(0, typeDefeninition.IndexOf("`"));
            return unmangledName + "<" + string.Join(",", genericArguments.Select(PrettyName)) + ">";
        }
    }
}
