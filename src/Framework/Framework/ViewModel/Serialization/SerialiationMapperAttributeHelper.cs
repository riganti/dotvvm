using System;
using System.Reflection;
using STJ = System.Text.Json.Serialization;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public static class SerialiationMapperAttributeHelper
    {
        static readonly Type? JsonConstructorNJ = Type.GetType("Newtonsoft.Json.JsonConstructorAttribute, Newtonsoft.Json");
        static readonly Type? JsonIgnoreNJ = Type.GetType("Newtonsoft.Json.JsonIgnoreAttribute, Newtonsoft.Json");
        static readonly Type? JsonConverterNJ = Type.GetType("Newtonsoft.Json.JsonConverterAttribute, Newtonsoft.Json");

        public static bool IsJsonConstructor(MethodBase constructor) =>
            constructor.IsDefined(typeof(STJ.JsonConstructorAttribute)) ||
                (JsonConstructorNJ is { } && constructor.IsDefined(JsonConstructorNJ));

        public static bool IsJsonIgnore(MemberInfo member)
        {
            if (JsonIgnoreNJ is {} && member.IsDefined(JsonIgnoreNJ))
                return true;
            if (member.GetCustomAttribute<STJ.JsonIgnoreAttribute>() is {} ignore)
                return ignore.Condition == STJ.JsonIgnoreCondition.Always;
            return false;
        }

        public static bool HasNewtonsoftJsonConvert(MemberInfo member) =>
            JsonConverterNJ is { } && member.IsDefined(JsonConverterNJ);
    }
}
