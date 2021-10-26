using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Compilation.Binding
{
    public static class StaticCommandExecutionPlanSerializer
    {
        public static JToken SerializePlan(StaticCommandInvocationPlan plan)
        {
            var hasOverloads = HasOverloads(plan.Method);
            var array = new JArray(
                new JValue(GetTypeFullName(plan.Method.DeclaringType)),
                new JValue(plan.Method.Name),
                new JArray(plan.Method.GetGenericArguments().Select(GetTypeFullName)),
                new JValue(plan.Method.GetParameters().Length),
                hasOverloads ? new JArray(plan.Method.GetParameters().Select(p => GetTypeFullName(p.ParameterType))) : JValue.CreateNull(),
                JToken.FromObject(plan.Arguments.Select(a => (byte)a.Type).ToArray())
            );
            var parameters = (new ParameterInfo[plan.Method.IsStatic ? 0 : 1]).Concat(plan.Method.GetParameters()).ToArray();
            foreach (var (arg, parameter) in plan.Arguments.Zip(parameters, (a, b) => (a, b)))
            {
                if (arg.Type == StaticCommandParameterType.Argument)
                {
                    if ((parameter?.ParameterType ?? plan.Method.DeclaringType).Equals(arg.Arg))
                        array.Add(JValue.CreateNull());
                    else
                        array.Add(new JValue(arg.Arg!.CastTo<Type>().Apply(GetTypeFullName)));
                }
                else if (arg.Type == StaticCommandParameterType.Constant)
                {
                    array.Add(JToken.FromObject(arg.Arg));
                }
                else if (arg.Type == StaticCommandParameterType.DefaultValue)
                {
                    array.Add(JValue.CreateNull());
                }
                else if (arg.Type == StaticCommandParameterType.Inject)
                {
                    if ((parameter?.ParameterType ?? plan.Method.DeclaringType).Equals(arg.Arg))
                        array.Add(JValue.CreateNull());
                    else
                        array.Add(new JValue(arg.Arg!.CastTo<Type>().Apply(GetTypeFullName)));
                }
                else if (arg.Type == StaticCommandParameterType.Invocation)
                {
                    array.Add(SerializePlan((StaticCommandInvocationPlan)arg.Arg!));
                }
                else throw new NotSupportedException(arg.Type.ToString());
            }
            while (array.Last.Type == JTokenType.Null)
                array.Last.Remove();
            return array;
        }

        private static bool HasOverloads(MethodInfo method)
        {
            return method.DeclaringType.GetMethods().Where(m => m.Name == method.Name && m.GetParameters().Length == method.GetParameters().Length).Take(2).Count() > 1;
        }

        private static string GetTypeFullName(Type type) => $"{type.FullName}, {type.Assembly.GetName().Name}";

        public static byte[] EncryptJson(JToken json, IViewModelProtector protector)
        {
            var stream = new MemoryStream();
            using (var writer = new JsonTextWriter(new StreamWriter(stream)))
            {
                json.WriteTo(writer);
            }
            return protector.Protect(stream.ToArray(), GetEncryptionPurposes());
        }
        public static string[] GetEncryptionPurposes()
        {
            return new[] {
                "StaticCommand",
            };
        }
        public static StaticCommandInvocationPlan DeserializePlan(JToken planInJson)
        {
            var jarray = (JArray)planInJson;
            var typeName = jarray[0].Value<string>();
            var methodName = jarray[1].Value<string>();
            var genericTypeNames = jarray[2].Value<JArray>();
            var parametersCount = jarray[3].Value<int>();
            var parameterTypeNames = jarray[4].Value<JArray>();
            var hasOtherOverloads = parameterTypeNames != null;
            var argTypes = jarray[5].ToObject<byte[]>().Select(a => (StaticCommandParameterType)a).ToArray();

            MethodInfo method;
            if (hasOtherOverloads)
            {
                // There are multiple overloads available, therefore exact parameters need to be resolved first
                var parameters = parameterTypeNames.Select(n => Type.GetType(n.Value<string>())).ToArray();
                method = Type.GetType(typeName).GetMethod(methodName, parameters);
            }
            else
            {
                // There are no overloads
                method = Type.GetType(typeName).GetMethods().SingleOrDefault(m => m.Name == methodName && m.GetParameters().Length == parametersCount);
            }
            
            if (method == null || !method.IsDefined(typeof(AllowStaticCommandAttribute)))
                throw new NotSupportedException("The specified method was not found or is not allowed to be used within a static command.");

            if (method.IsGenericMethod)
            {
                var generics = genericTypeNames.Select(n => Type.GetType(n.Value<string>())).ToArray();
                method = method.MakeGenericMethod(generics);
            }

            var methodParameters = method.GetParameters();
            var args = argTypes
                .Select((a, i) => (type: a, arg: jarray.Count <= i + 6 ? JValue.CreateNull() : jarray[i + 6], parameter: (method.IsStatic ? methodParameters[i] : (i == 0 ? null : methodParameters[i - 1]))))
                .Select((a) => {
                    switch (a.type)
                    {
                        case StaticCommandParameterType.Argument:
                        case StaticCommandParameterType.Inject:
                            if (a.arg.Type == JTokenType.Null)
                                return new StaticCommandParameterPlan(a.type, a.parameter?.ParameterType ?? method.DeclaringType);
                            else
                                return new StaticCommandParameterPlan(a.type, a.arg.Value<string>().Apply(Type.GetType));
                        case StaticCommandParameterType.Constant:
                            return new StaticCommandParameterPlan(a.type, a.arg.ToObject(a.parameter?.ParameterType ?? method.DeclaringType));
                        case StaticCommandParameterType.DefaultValue:
                            return new StaticCommandParameterPlan(a.type, a.parameter?.DefaultValue);
                        case StaticCommandParameterType.Invocation:
                            return new StaticCommandParameterPlan(a.type, DeserializePlan(a.arg));
                        default:
                            throw new NotSupportedException($"{a.type}");
                    }
                }).ToArray();
            return new StaticCommandInvocationPlan(method, args);
        }


        public static JToken DecryptJson(byte[] data, IViewModelProtector protector)
        {
            using (var reader = new JsonTextReader(new StreamReader(new MemoryStream(protector.Unprotect(data, GetEncryptionPurposes())))))
            {
                return JToken.ReadFrom(reader);
            }
        }
    }
}
