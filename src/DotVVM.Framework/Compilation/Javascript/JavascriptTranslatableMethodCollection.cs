using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.HelperNamespace;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class DelegateInvokeMethodTranslator : IJavascriptMethodTranslator
    {
        public JsExpression TryTranslateCall(LazyTranslatedExpression context, LazyTranslatedExpression[] arguments, MethodInfo method)
        {
            if (method == null)
            {
                return null;
            }

            if (method.Name == "Invoke" && typeof(Delegate).IsAssignableFrom(method.DeclaringType))
            {
                var invocationTargetExpresionCall = context.JsExpression().Invoke(arguments.Select(a => a.JsExpression()));
                return invocationTargetExpresionCall
                    .WithAnnotation(new ResultIsPromiseAnnotation(a=> new JsIdentifierExpression("Promise").Member("resolve").Invoke(a)));
            }
            return null;
        }
    }

    public class JavascriptTranslatableMethodCollection : IJavascriptMethodTranslator
    {
        public readonly Dictionary<MethodInfo, IJavascriptMethodTranslator> MethodTranslators = new Dictionary<MethodInfo, IJavascriptMethodTranslator>();
        public readonly HashSet<Type> Interfaces = new HashSet<Type>();

        public JavascriptTranslatableMethodCollection()
        {
            AddDefaultMethodTranslators();
        }

        public void AddMethodTranslator(Type declaringType, string methodName, IJavascriptMethodTranslator translator, Type[] parameters = null, bool allowGeneric = true, bool allowMultipleMethods = false)
        {
            var methods = declaringType.GetMethods()
                .Where(m => m.Name == methodName && (allowGeneric || !m.IsGenericMethod));
            if (parameters != null)
            {
                methods = methods.Where(m => {
                    var mp = m.GetParameters();
                    return mp.Length == parameters.Length && parameters.Zip(mp, (specified, method) => method.ParameterType.IsAssignableFrom(specified)).All(t => t);
                });
            }

            AddMethodsCore(methods.ToArray(), translator, allowMultipleMethods);
        }

        public void AddMethodTranslator(Type declaringType, string methodName, IJavascriptMethodTranslator translator, int parameterCount, bool allowMultipleMethods = false)
        {
            var methods = declaringType.GetMethods()
                .Where(m => m.Name == methodName)
                .Where(m => m.GetParameters().Length == parameterCount)
                .ToArray();

            AddMethodsCore(methods, translator, allowMultipleMethods);
        }

        private void AddMethodsCore(MethodInfo[] methodsList, IJavascriptMethodTranslator translator, bool allowMultipleMethods)
        {
            if (methodsList.Length > 1 && !allowMultipleMethods) throw new Exception("More then one method was found.");
            if (methodsList.Length == 0) throw new Exception("No methods found.");
            foreach (var method in methodsList)
            {
                AddMethodTranslator(method, translator);
            }
        }

        public void AddMethodTranslator(MethodInfo method, IJavascriptMethodTranslator translator)
        {
            MethodTranslators.Add(method, translator);
            if (method.DeclaringType.GetTypeInfo().IsInterface)
                Interfaces.Add(method.DeclaringType);
        }

        public void AddPropertySetterTranslator(Type declaringType, string methodName, IJavascriptMethodTranslator translator)
        {
            var property = declaringType.GetProperty(methodName);
            AddMethodTranslator(property.SetMethod, translator);
        }

        public void AddPropertyGetterTranslator(Type declaringType, string methodName, IJavascriptMethodTranslator translator)
        {
            var property = declaringType.GetProperty(methodName);
            AddMethodTranslator(property.GetMethod, translator);
        }

        static bool ToStringCheck(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Convert) expr = ((UnaryExpression)expr).Operand;
            return expr.Type.GetTypeInfo().IsPrimitive;
        }

        public static JsExpression BuildIndexer(JsExpression target, JsExpression index, MemberInfo member) =>
            target.Indexer(index).WithAnnotation(new VMPropertyInfoAnnotation { MemberInfo = member });

        public void AddDefaultMethodTranslators()
        {
            var lengthMethod = new GenericMethodCompiler(a => a[0].Member("length"));
            AddPropertyGetterTranslator(typeof(Array), nameof(Array.Length), lengthMethod);
            AddPropertyGetterTranslator(typeof(ICollection), nameof(ICollection.Count), lengthMethod);
            AddPropertyGetterTranslator(typeof(ICollection<>), nameof(ICollection.Count), lengthMethod);
            AddPropertyGetterTranslator(typeof(string), nameof(string.Length), lengthMethod);
            AddMethodTranslator(typeof(Enumerable), "Count", parameterCount: 1, translator: new GenericMethodCompiler(a => a[1].Member("length")));
            AddMethodTranslator(typeof(object), "ToString", new GenericMethodCompiler(
                a => new JsIdentifierExpression("String").Invoke(a[0]), (m, c, a) => ToStringCheck(c)), 0);
            AddMethodTranslator(typeof(Convert), "ToString", new GenericMethodCompiler(
                a => new JsIdentifierExpression("String").Invoke(a[1]), (m, c, a) => ToStringCheck(a[0])), 1, true);
            AddMethodTranslator(typeof(Enums), "GetNames", new EnumGetNamesMethodTranslator(), 0);

            JsExpression indexer(JsExpression[] args, MethodInfo method) =>
                BuildIndexer(args[0], args[1], method.DeclaringType.GetProperty("Item"));
            AddMethodTranslator(typeof(IList), "get_Item", new GenericMethodCompiler(indexer));
            AddMethodTranslator(typeof(IList<>), "get_Item", new GenericMethodCompiler(indexer));
            AddMethodTranslator(typeof(List<>), "get_Item", new GenericMethodCompiler(indexer));
            AddMethodTranslator(typeof(Enumerable).GetMethod("ElementAt", BindingFlags.Static | BindingFlags.Public), new GenericMethodCompiler((args, method) =>
                BuildIndexer(args[1], args[2], method)));
            AddPropertyGetterTranslator(typeof(Nullable<>), "Value", new GenericMethodCompiler((args, method) => args[0]));
            AddPropertyGetterTranslator(typeof(Nullable<>), "HasValue",
                new GenericMethodCompiler(args => new JsBinaryExpression(args[0], BinaryOperatorType.NotEqual, new JsLiteral(null))));
            //AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Count), lengthMethod, new[] { typeof(IEnumerable) });

            BindingApi.RegisterJavascriptTranslations(this);
            BindingPageInfo.RegisterJavascriptTranslations(this);
            BindingCollectionInfo.RegisterJavascriptTranslations(this);

            // string formatting
            var stringFormatTranslator = new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("format").Invoke(args[1], new JsArrayExpression(args.Skip(2)))
            );
            // TODO: string.Format could be two-way
            AddMethodTranslator(typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object) }), stringFormatTranslator);
            AddMethodTranslator(typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object), typeof(object) }), stringFormatTranslator);
            AddMethodTranslator(typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object), typeof(object), typeof(object) }), stringFormatTranslator);
            AddMethodTranslator(typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object[]) }), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("format").Invoke(args[1], args[2])
            ));
            AddMethodTranslator(typeof(DateTime).GetMethod("ToString", Type.EmptyTypes), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
            ));
            AddMethodTranslator(typeof(DateTime).GetMethod("ToString", new[] { typeof(string) }), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])
            ));
            AddMethodTranslator(typeof(DateTime?).GetMethod("ToString", Type.EmptyTypes), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
            ));
            AddMethodTranslator(typeof(Guid).GetMethod("ToString", Type.EmptyTypes), new GenericMethodCompiler(args => args[0]));

            foreach (var num in ReflectionUtils.NumericTypes.Except(new[] { typeof(char) }))
            {
                AddMethodTranslator(num.GetMethod("ToString", Type.EmptyTypes), new GenericMethodCompiler(
                    args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingNumberToString")
                            .WithAnnotation(new GlobalizeResourceBindingProperty())
                            .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                            .WithAnnotation(ResultIsObservableAnnotation.Instance)
                ));
                AddMethodTranslator(num.GetMethod("ToString", new[] { typeof(string) }), new GenericMethodCompiler(
                    args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingNumberToString")
                            .WithAnnotation(new GlobalizeResourceBindingProperty())
                            .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])
                            .WithAnnotation(ResultIsObservableAnnotation.Instance)
                ));
                AddMethodTranslator(typeof(Nullable<>).MakeGenericType(num).GetMethod("ToString", Type.EmptyTypes), new GenericMethodCompiler(
                    args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingNumberToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
                ));
            }

            AddPropertyGetterTranslator(typeof(Task<>), "Result", new GenericMethodCompiler(args => FunctionalExtensions.ApplyAction(args[0], a => a.RemoveAnnotations(typeof(ViewModelInfoAnnotation)))));

            AddMethodTranslator(typeof(DotvvmBindableObject).GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(m => m.Name == "GetValue" && !m.ContainsGenericParameters), new GenericMethodCompiler(
                args => {
                    var dotvvmproperty = ((DotvvmProperty)((JsLiteral)args[1]).Value);
                    return JavascriptTranslationVisitor.TranslateViewModelProperty(args[0], (MemberInfo)dotvvmproperty.PropertyInfo ?? dotvvmproperty.PropertyType.GetTypeInfo(), name: dotvvmproperty.Name);
                }
            ));
        }

        public JsExpression TryTranslateCall(LazyTranslatedExpression context, LazyTranslatedExpression[] args, MethodInfo method)
        {
            if (method == null)
            {
                return null;
            }

            {
                if (MethodTranslators.TryGetValue(method, out var translator) && translator.TryTranslateCall(context, args, method) is JsExpression result)
                {
                    return result;
                }
            }

            if (method.IsGenericMethod)
            {
                var genericMethod = method.GetGenericMethodDefinition();
                if (MethodTranslators.TryGetValue(genericMethod, out var translator) && translator.TryTranslateCall(context, args, method) is JsExpression result)
                    return result;
            }

            foreach (var iface in method.DeclaringType.GetInterfaces())
            {
                if (Interfaces.Contains(iface))
                {
                    var map = method.DeclaringType.GetTypeInfo().GetRuntimeInterfaceMap(iface);
                    var imIndex = Array.IndexOf(map.TargetMethods, method);
                    if (imIndex >= 0 && MethodTranslators.TryGetValue(map.InterfaceMethods[imIndex], out var translator) && translator.TryTranslateCall(context, args, method) is JsExpression result)
                        return result;
                }
            }
            if (method.DeclaringType.GetTypeInfo().IsGenericType && !method.DeclaringType.GetTypeInfo().IsGenericTypeDefinition)
            {
                var genericType = method.DeclaringType.GetGenericTypeDefinition();
                var m2 = genericType.GetMethod(method.Name,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                    binder: null, types: method.GetParameters().Select(p => p.ParameterType).ToArray(), modifiers: null);

                if (m2 != null)
                {
                    var r2 = TryTranslateCall(context, args, m2);
                    if (r2 != null) return r2;
                }
            }
            if (method.DeclaringType == typeof(Array))
            {
                var m2 = typeof(Array).GetMethod(method.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (m2 != null)
                {
                    var r2 = TryTranslateCall(context, args, m2);
                    if (r2 != null) return r2;
                }
            }
            var baseMethod = method.GetBaseDefinition();
            if (baseMethod != null && baseMethod != method) return TryTranslateCall(context, args, baseMethod);
            else return null;
        }
    }
}
