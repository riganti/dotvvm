using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;

namespace DotVVM.Compiler
{
    public class RefObjectSerializer
    {
        public static ParameterExpression ServiceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        public static ParameterExpression DotvvmConfigurationParameter = Expression.Parameter(typeof(DotvvmConfiguration), "config");
        public static Expression BindingCompilationService = Expression.Convert(Expression.Call(ServiceProviderParameter, nameof(IServiceProvider.GetService), Type.EmptyTypes, Expression.Constant(typeof(BindingCompilationService), typeof(Type))), typeof(BindingCompilationService));
        public static readonly ConcurrentDictionary<object, Expression> KnownObjects = new ConcurrentDictionary<object, Expression>();
        static RefObjectSerializer()
        {
            var objects = ReflectionUtils.GetLoadableTypes(typeof(DotvvmProperty).Assembly)
                .Where(t => !t.IsGenericTypeDefinition)
                .SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.Public))
                .Where(f => f.IsInitOnly)
                .Select(f => new KeyValuePair<object, Expression>(f.GetValue(null), Expression.Field(null, f)));
            foreach (var obj in objects)
                KnownObjects.TryAdd(obj.Key, obj.Value);
        }

        public RefObjectSerializer()
        {

        }

        ConcurrentDictionary<object, string> requiredObjects = new ConcurrentDictionary<object, string>();
        int counter = 0;

        public string AddObject(object obj) =>
            requiredObjects.GetOrAdd(obj, _ => "Obj_" + _.GetType().Name + "_" + Interlocked.Increment(ref counter));

        public static void MapObject(object obj, Expression target, Dictionary<object, Expression> result)
        {
            if (obj == null || obj.GetType().IsPrimitive || obj is string) return;
            if (target.Type != obj.GetType().GetPublicBaseType()) target = Expression.Convert(target, obj.GetType().GetPublicBaseType());
            result.Add(obj, target);
            if (obj is System.Collections.IList list)
            {
                foreach (var (i, item) in list.OfType<object>().Indexed())
                {
                    if (item == null) continue;
                    var objExpression = Expression.Convert(
                            Expression.MakeIndex(target, typeof(System.Collections.IList).GetProperty("Item"), new[] { Expression.Constant(i) }),
                            item.GetType()
                        );
                    if (!result.ContainsKey(item))
                    {
                        MapObject(item, objExpression, result);
                    }
                }
            }
            else if (obj is System.Collections.IDictionary dictionary)
            {
                foreach (var (i, key) in dictionary.Keys.OfType<object>().Indexed())
                {
                    if (key == null || dictionary[key] == null) continue;
                    var item = dictionary[key];
                    var objExpression = Expression.Convert(
                            Expression.MakeIndex(target, typeof(System.Collections.IDictionary).GetProperty("Item"), new[] { Expression.Constant(key, typeof(object)) }),
                            item.GetType()
                        );
                    if (!result.ContainsKey(item))
                    {
                        MapObject(item, objExpression, result);
                    }
                }
            }
            else
            {
                if (obj.GetType().Assembly == typeof(string).Assembly) return;
                if (obj.GetType().Assembly == typeof(Expression).Assembly) return;
                if (obj is JsExpression) return;
                foreach (var prop in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (prop.GetIndexParameters().Length != 0) continue;
                    var item = prop.GetValue(obj);
                    if (item == null) continue;
                    var objExpression = Expression.Property(target, prop);
                    if (!result.ContainsKey(item))
                    {
                        MapObject(item, objExpression, result);
                    }
                }
                foreach (var field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    var item = field.GetValue(obj);
                    if (item == null) continue;
                    var objExpression = Expression.Field(target, field);
                    if (!result.ContainsKey(item))
                    {
                        MapObject(item, objExpression, result);
                    }
                }
            }
        }

        public (Expression builder, ParameterExpression[] fields) CreateBuilder(DotvvmConfiguration configuration)
        {
            var dict = new Dictionary<object, Expression>();
            var callStack = new HashSet<object>();
            var locals = new List<ParameterExpression>();
            var results = new List<ParameterExpression>();
            var body = new List<Expression>();
            var dedupCache = new Dictionary<Expression, ParameterExpression>(ExpressionComparer.Instance);
            //var presetObjects = new Dictionary<object, Expression>();
            MapObject(configuration, DotvvmConfigurationParameter, dict);

            Expression serializeObject(object obj, Type expectedType)
            {
                if (obj == null)
                    return Expression.Constant(null, expectedType);
                if (dict.TryGetValue(obj, out var rrrr)) return rrrr;
                if (!callStack.Add(obj)) throw new NotSupportedException($"Reference cycles are not supported.");
                var objType = obj.GetType();
                ParameterExpression ret(Expression e, bool thisobj = true)
                {
                    if (dedupCache.TryGetValue(e, out var p)) return p;
                    p = Expression.Parameter(e.Type);
                    body.Add(Expression.Assign(p, e));
                    if (thisobj) dict.Add(obj, p);
                    locals.Add(p);
                    dedupCache.Add(e, p);
                    return p;
                }
                try
                {
                    if (KnownObjects.TryGetValue(obj, out var result))
                        return result;
                    else if (objType.IsPrimitive || obj is string || obj is MemberInfo)
                        return Expression.Constant(obj, objType.GetPublicBaseType());
                    else if (obj is System.Collections.IEnumerable collection)
                    {
                        var element = ReflectionUtils.GetEnumerableType(collection.GetType());
                        Expression expr = collection.OfType<object>()
                            .Select(e => serializeObject(e, element))
                            .Apply(c => Expression.NewArrayInit(element, c));
                        if (!objType.IsArray)
                        {
                            var targetType = objType.Name == "ImmutableList`1" ? typeof(ImmutableList) :
                                //objType.Name == "ImmutableDictionary" ? typeof(ImmutableDictionary) :
                                typeof(ImmutableArray);
                            expr = expr.Apply(c => Expression.Call(null, targetType
                                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                                .First(m => (m.Name == "Create" || m.Name == "To" + targetType.Name) && m.GetParameters().FirstOrDefault()?.ParameterType.IsArray == true)
                                .MakeGenericMethod(new[] { element }),
                                c
                            ));
                        }
                        return Expression.Convert(ret(expr), expectedType);
                    }
                    else if (obj is Expression expression)
                        return ret(Expression.Quote(expression));
                    else if (obj is Delegate deleg)
                    {
                        if (!translatedDelegates.TryGetValue(deleg, out var method)) throw new NotSupportedException("Could not serialize delegate");
                        var (sanitizedMethod, parameters) = UnallowedConstantRemover.ReplaceBadConstants(method);
                        return ret(ExpressionUtils.Replace(Expression.Lambda(sanitizedMethod, parameters.Select(l => l.Item1)), parameters.Select(p => serializeObject(p.Item2, p.Item1.Type)).ToArray()));
                    }
                    else if (obj is IBinding binding)
                    {
                        var properties = BindingCompiler.GetMinimalCloneProperties(binding)
                            .Select(p => serializeObject(p, p.GetType()))
                            .Apply(p => Expression.NewArrayInit(typeof(object), p));

                        return ret(Expression.New(
                            binding.GetType().GetConstructor(new[] { typeof(BindingCompilationService), typeof(IEnumerable<object>) }),
                            ret(BindingCompilationService, thisobj: false),
                            properties
                        ));
                    }
                    else
                    {
                        var map = Map(objType.GetPublicBaseType());
                        var ctorArgs = map.CtorArguments.Select(p => serializeObject(Evaluate(p, obj), p.ReturnType)).ToArray();
                        var newObj = ExpressionUtils.Replace(map.Ctor, ctorArgs);
                        if (!newObj.Type.IsValueType || map.Properties.Any()) newObj = ret(newObj);
                        foreach (var prop in map.Properties)
                        {
                            body.Add(ExpressionUtils.Replace(prop.Setter, newObj, serializeObject(Evaluate(prop.Getter, obj), prop.Getter.ReturnType)));
                        }
                        return newObj;
                    }
                }
                finally
                {
                    callStack.Remove(obj);
                }
            }

            foreach (var req in requiredObjects)
            {
                var expr = serializeObject(req.Key, req.GetType().GetPublicBaseType());
                var field = Expression.Parameter(req.Key.GetType().GetPublicBaseType(), req.Value);
                body.Add(Expression.Assign(field, expr));
                results.Add(field);
            }
            return (Expression.Block(locals, body), results.ToArray());
        }

        static object Evaluate(LambdaExpression expr, params object[] parameters)
        {
            if (expr.Parameters.Count == 1 && expr.Body is MemberExpression memberExpr && memberExpr.Expression == expr.Parameters.Single())
            {
                if (memberExpr.Member is PropertyInfo prop) return prop.GetValue(parameters.Single());
                else if (memberExpr.Member is FieldInfo field) return field.GetValue(parameters.Single());
            }
            if (expr.Parameters.Count == 1 && expr.Body == expr.Parameters.Single()) return parameters[0];
            return expr.Compile().DynamicInvoke(parameters);
        }

        SerializationObjectMap Map(Type type)
        {
            if (type.IsAbstract || type.IsInterface || type == typeof(object)) throw new NotSupportedException($"Can not instantiate instance of {type}.");

            if (typeof(Expression).IsAssignableFrom(type))
                return new SerializationObjectMap(null, CreateLambda(Expression.Quote, type), CreateLambda(e => e, type).Apply(ImmutableArray.Create));
            if (type == typeof(ParametrizedCode))
            {
                var ctorArgs = new[] { typeof(string[]), typeof(CodeParameterInfo[]), typeof(OperatorPrecedence) };
                return new SerializationObjectMap(
                    new SerializationPropertyMap[0],
                    CreateLambda(args => Expression.New(typeof(ParametrizedCode).GetConstructor(ctorArgs), args), ctorArgs),
                    new[] {
                        CreateLambda(e => Expression.Field(e, "stringParts"), typeof(ParametrizedCode)),
                        CreateLambda(e => Expression.Field(e, "parameters"), typeof(ParametrizedCode)),
                        CreateLambda(e => Expression.Field(e, "OperatorPrecedence"), typeof(ParametrizedCode)),
                    });
            }
            else
            {
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                var settableProps =
                    properties.Where(p => p.GetMethod?.IsPublic == true && p.SetMethod?.IsPublic == true)
                    .Select(p => new SerializationPropertyMap(
                        CreateLambda(e => Expression.Property(e, p), type),
                        CreateLambda((e, val) => Expression.Assign(Expression.Property(e, p), val), type, p.PropertyType)
                    ));
                var ctor = (MethodBase)type.GetMethod("refserializer_create", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ?? (MethodBase)type.GetConstructors().SingleOrDefault() ?? type.GetMethod("Create", BindingFlags.Static | BindingFlags.Public);
                var ctorParameters = ctor.GetParameters();
                var ctorProperties =
                    properties.Where(p => p.GetMethod?.IsPublic == true)
                        .Where(p => p.SetMethod?.IsPublic != true)
                        .Select(p => new {
                            p,
                            arg = ctorParameters.FirstOrDefault(a => p.Name.Equals(a.Name, StringComparison.OrdinalIgnoreCase))
                        })
                        .Where(t => t.arg != null)
                        .Select(t => (parameter: t.arg, getter: CreateLambda(e => Expression.Property(e, t.p), type)))
                    .Concat(fields.Select(f => new {
                        f,
                        arg = ctorParameters.FirstOrDefault(a => f.Name.Equals(a.Name, StringComparison.OrdinalIgnoreCase))
                    })
                        .Where(t => t.arg != null)
                        .Select(t => (parameter: t.arg, getter: CreateLambda(e => Expression.Field(e, t.f), type)))).ToDictionary(p => p.parameter, p => p.getter);
                var ctorArgs = ctorParameters.Select(p => ctorProperties[p]);

                return new SerializationObjectMap(
                    settableProps,
                    CreateLambda(a => ctor is ConstructorInfo c ? (Expression)Expression.New(c, a) : Expression.Call(null, (MethodInfo)ctor, a), ctorParameters.Select(p => p.ParameterType).ToArray()),
                    ctorArgs
                );
            }
        }

        static LambdaExpression CreateLambda(Func<Expression, Expression> expr, Type t)
        {
            var param = Expression.Parameter(t);
            return Expression.Lambda(expr(param), param);
        }
        static LambdaExpression CreateLambda(Func<Expression, Expression, Expression> expr, Type t1, Type t2)
        {
            var param1 = Expression.Parameter(t1);
            var param2 = Expression.Parameter(t2);
            return Expression.Lambda(expr(param1, param2), param1, param2);
        }
        static LambdaExpression CreateLambda(Func<Expression[], Expression> expr, Type[] t)
        {
            var param = t.Select(Expression.Parameter).ToArray();
            return Expression.Lambda(expr(param), param);
        }

        static ConditionalWeakTable<Delegate, LambdaExpression> translatedDelegates = new ConditionalWeakTable<Delegate, LambdaExpression>();
        public static void RegisterDelegateTranslation(Delegate d, LambdaExpression method)
        {
            translatedDelegates.Add(d, method);
        }

        class SerializationPropertyMap
        {
            public LambdaExpression Getter { get; }
            public LambdaExpression Setter { get; }

            public SerializationPropertyMap(
                LambdaExpression getter,
                LambdaExpression setter)
            {
                this.Getter = getter;
                this.Setter = setter;
            }
        }

        class SerializationObjectMap
        {
            public ImmutableArray<SerializationPropertyMap> Properties { get; }
            public LambdaExpression Ctor { get; }
            public ImmutableArray<LambdaExpression> CtorArguments { get; }

            public SerializationObjectMap(
                IEnumerable<SerializationPropertyMap> properties,
                LambdaExpression ctor,
                IEnumerable<LambdaExpression> ctorArguments)
            {
                this.Properties = properties?.ToImmutableArray() ?? ImmutableArray<SerializationPropertyMap>.Empty;
                this.Ctor = ctor;
                this.CtorArguments = ctorArguments?.ToImmutableArray() ?? ImmutableArray<LambdaExpression>.Empty;
            }
        }
    }
}
