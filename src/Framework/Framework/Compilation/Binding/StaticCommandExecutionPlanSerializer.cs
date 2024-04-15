using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Compilation.Binding
{
    public static class StaticCommandExecutionPlanSerializer
    {
        public static JsonNode SerializePlan(StaticCommandInvocationPlan plan)
        {
            var hasOverloads = HasOverloads(plan.Method);
            var array = new JsonArray(
                JsonValue.Create(GetTypeFullName(plan.Method.DeclaringType!)),
                JsonValue.Create(plan.Method.Name),
                new JsonArray(plan.Method.GetGenericArguments().Select(t => JsonValue.Create(GetTypeFullName(t))).ToArray()),
                JsonValue.Create(plan.Method.GetParameters().Length),
                hasOverloads
                    ? new JsonArray(plan.Method.GetParameters().Select(p => JsonValue.Create(GetTypeFullName(p.ParameterType))).ToArray())
                    : null,
                JsonValue.Create(Convert.ToBase64String(plan.Arguments.Select(a => (byte)a.Type).ToArray()))
            );
            var parameters = (new ParameterInfo[plan.Method.IsStatic ? 0 : 1]).Concat(plan.Method.GetParameters()).ToArray();
            foreach (var (arg, parameter) in plan.Arguments.Zip(parameters, (a, b) => (a, b)))
            {
                if (arg.Type == StaticCommandParameterType.Argument)
                {
                    if ((parameter?.ParameterType ?? plan.Method.DeclaringType!).Equals(arg.Arg))
                        array.Add(null);
                    else
                        array.Add(JsonValue.Create(GetTypeFullName((Type)arg.Arg!)));
                }
                else if (arg.Type == StaticCommandParameterType.Constant)
                {
                    array.Add(arg.Arg is null ? null : JsonSerializer.SerializeToNode(arg.Arg, parameter.ParameterType));
                }
                else if (arg.Type == StaticCommandParameterType.DefaultValue)
                {
                    array.Add(null);
                }
                else if (arg.Type == StaticCommandParameterType.Inject)
                {
                    if ((parameter?.ParameterType ?? plan.Method.DeclaringType!).Equals(arg.Arg))
                        array.Add(null);
                    else
                        array.Add(JsonValue.Create(GetTypeFullName((Type)arg.Arg!)));
                }
                else if (arg.Type == StaticCommandParameterType.Invocation)
                {
                    array.Add(SerializePlan((StaticCommandInvocationPlan)arg.Arg!));
                }
                else throw new NotSupportedException(arg.Type.ToString());
            }
            return array;
        }

        private static bool HasOverloads(MethodInfo method)
        {
            return method.DeclaringType!.GetMethods().Where(m => m.Name == method.Name && m.GetParameters().Length == method.GetParameters().Length).Take(2).Count() > 1;
        }

        private static string GetTypeFullName(Type type) => $"{type.FullName}, {type.Assembly.GetName().Name}";

        public static byte[] EncryptJson(JsonNode json, IViewModelProtector protector)
        {
            var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                json.WriteTo(writer);
            }
            return protector.Protect(stream.ToArray(), GetEncryptionPurposes());
        }
        public static string[] GetEncryptionPurposes()
        {
            return [ "StaticCommand" ];
        }

        static string?[]? ReadStringArray(ref Utf8JsonReader json)
        {
            if (json.TokenType == JsonTokenType.Null)
            {
                json.AssertRead();
                return null;
            }

            json.AssertRead(JsonTokenType.StartArray);
            var result = new List<string?>();
            while (true)
            {
                if (json.TokenType == JsonTokenType.EndArray)
                {
                    json.AssertRead();
                    return result.ToArray();
                }
                result.Add(json.ReadString());
            }
        }
        public static StaticCommandInvocationPlan DeserializePlan(ref Utf8JsonReader json)
        {
            if (json.TokenType == JsonTokenType.None)
            {
                json.AssertRead();
            }
            json.AssertRead(JsonTokenType.StartArray);
            var declaringType = Type.GetType(json.ReadString().NotNull());
            var methodName = json.ReadString().NotNull();
            var genericTypeNames = ReadStringArray(ref json);
            var parametersCount = json.GetInt32();
            json.AssertRead();
            var parameterTypeNames = ReadStringArray(ref json);
            var hasOtherOverloads = parameterTypeNames != null;
            var argTypes = json.GetBytesFromBase64();
            json.AssertRead();

            MethodInfo? method;
            if (hasOtherOverloads)
            {
                // There are multiple overloads available, therefore exact parameters need to be resolved first
                var parameters = parameterTypeNames!.Select(n => Type.GetType(n.NotNull()).NotNull()).ToArray();
                method = declaringType?.GetMethod(methodName, parameters);
            }
            else
            {
                // There are no overloads
                method = declaringType?.GetMethods().SingleOrDefault(m => m.Name == methodName && m.GetParameters().Length == parametersCount);
            }
            
            if (method == null || !method.IsDefined(typeof(AllowStaticCommandAttribute)))
                throw new NotSupportedException("The specified method was not found or is not allowed to be used within a static command.");

            if (method.IsGenericMethod)
            {
                var generics = genericTypeNames.NotNull().Select(n => Type.GetType(n.NotNull()).NotNull()).ToArray();
                method = method.MakeGenericMethod(generics);
            }

            var methodParameters = method.GetParameters();
            if (!method.IsStatic)
            {
                methodParameters = [ null, ..methodParameters ];
            }
            var args = new StaticCommandParameterPlan[methodParameters.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var type = (StaticCommandParameterType)argTypes[i];
                var paramType = methodParameters[i]?.ParameterType ?? method.DeclaringType.NotNull();
                args[i] = type switch
                {
                    StaticCommandParameterType.Argument or StaticCommandParameterType.Inject =>
                        new StaticCommandParameterPlan(type, json.TokenType == JsonTokenType.Null ? paramType : Type.GetType(json.GetString())),
                    StaticCommandParameterType.Constant =>
                        new StaticCommandParameterPlan(type, JsonSerializer.Deserialize(ref json, paramType)),
                    StaticCommandParameterType.DefaultValue =>
                        new StaticCommandParameterPlan(type, methodParameters[i].DefaultValue),
                    StaticCommandParameterType.Invocation =>
                        new StaticCommandParameterPlan(type, DeserializePlan(ref json)),
                    _ => throw new NotSupportedException(type.ToString())
                };
                json.AssertRead();
            }
            return new StaticCommandInvocationPlan(method, args);
        }


        public static byte[] DecryptJson(byte[] data, IViewModelProtector protector)
        {
            return protector.Unprotect(data, GetEncryptionPurposes());
        }
    }
}
