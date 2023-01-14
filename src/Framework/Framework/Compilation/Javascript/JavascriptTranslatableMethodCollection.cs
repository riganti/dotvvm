using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.HelperNamespace;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Generic = DotVVM.Framework.Compilation.Javascript.MethodFindingHelper.Generic;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class DelegateInvokeMethodTranslator : IJavascriptMethodTranslator
    {
        public JsExpression? TryTranslateCall(LazyTranslatedExpression? context, LazyTranslatedExpression[] arguments, MethodInfo method)
        {
            if (context is object && method.Name == "Invoke" && typeof(Delegate).IsAssignableFrom(method.DeclaringType))
            {
                var invocationTargetExpressionCall = context.JsExpression().Invoke(arguments.Select(a => a.JsExpression()));
                return invocationTargetExpressionCall
                    .WithAnnotation(new ResultIsPromiseAnnotation(a => new JsIdentifierExpression("Promise").Member("resolve").Invoke(a)) {
                        // If the delegate is called from value binding, just don't await and hope for the best
                        IsOptionalAwait = true,
                        // Promise.resolve is not needed when doing `await X`
                        IsPromiseGetterOptional = true
                    });
            }
            return null;
        }
    }

    public partial class JavascriptTranslatableMethodCollection : IJavascriptMethodTranslator
    {
        public readonly Dictionary<MethodInfo, IJavascriptMethodTranslator> MethodTranslators = new Dictionary<MethodInfo, IJavascriptMethodTranslator>();
        public readonly HashSet<Type> Interfaces = new HashSet<Type>();

        public JavascriptTranslatableMethodCollection()
        {
            AddDefaultMethodTranslators();
        }

        public void AddMethodTranslator(Type declaringType, string methodName, IJavascriptMethodTranslator translator, Type[]? parameters = null, bool allowGeneric = true, bool allowMultipleMethods = false)
        {
            var methods = declaringType.GetMethods()
                .Where(m => m.Name == methodName && (allowGeneric || !m.IsGenericMethod));
            if (parameters != null)
            {
                methods = methods.Where(m => {
                    var mp = m.GetParameters();
                    return mp.Length == parameters.Length && parameters.Zip(mp, (specified, method) => method.ParameterType == specified).All(t => t);
                });
            }

            AddMethodsCore(methods.ToArray(), translator, allowMultipleMethods);
        }

        public void AddMethodTranslator<T>(Expression<Func<T>> methodCall, IJavascriptMethodTranslator translator)
        {
            var method = (MethodInfo)MethodFindingHelper.GetMethodFromExpression(methodCall);
            AddMethodTranslator(method, translator);
        }

        public void AddMethodTranslator(Type declaringType, string methodName, IJavascriptMethodTranslator translator, int parameterCount, bool allowMultipleMethods = false, Func<ParameterInfo[], bool>? parameterFilter = null)
        {
            var methods = declaringType.GetMethods()
                .Where(m => m.Name == methodName)
                .Where(m => {
                    var parameters = m.GetParameters();
                    return parameters.Length == parameterCount && (parameterFilter?.Invoke(parameters) ?? true);
                })
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

        public void AddMethodTranslator([AllowNull] MethodInfo method, IJavascriptMethodTranslator translator)
        {
            if (method is null) throw new ArgumentNullException(nameof(method));
            MethodTranslators.Add(method, translator);
            if (method.DeclaringType!.IsInterface)
                Interfaces.Add(method.DeclaringType);
        }

        public void AddPropertySetterTranslator(Type declaringType, string propName, IJavascriptMethodTranslator translator)
        {
            var property = declaringType.GetProperty(propName);
            if (property is null)
                throw new Exception($"Property {declaringType}.{propName} does not exist.");
            if (property.SetMethod is null)
                throw new Exception($"Property {declaringType}.{propName} does not have a setter.");
            AddMethodTranslator(property.SetMethod, translator);
        }

        public void AddPropertyGetterTranslator(Type declaringType, string propName, IJavascriptMethodTranslator translator)
        {
            var property = declaringType.GetProperty(propName);
            if (property is null)
                throw new Exception($"Property {declaringType}.{propName} does not exist.");
            if (property.GetMethod is null)
                throw new Exception($"Property {declaringType}.{propName} does not have a getter.");
            AddMethodTranslator(property.GetMethod, translator);
        }

        public static JsExpression BuildIndexer(JsExpression target, JsExpression index, [AllowNull] MemberInfo member) =>
            target.Indexer(index).WithAnnotation(new VMPropertyInfoAnnotation(member.NotNull()));

        public void AddDefaultMethodTranslators()
        {
            var lengthMethod = new GenericMethodCompiler(a => a[0].Member("length"));
            // AddPropertyGetterTranslator(typeof(Array), nameof(Array.Length), lengthMethod);
            AddMethodTranslator(() => default(Array)!.Length, lengthMethod);
            AddMethodTranslator(() => default(ICollection)!.Count, lengthMethod);
            AddMethodTranslator(() => default(ICollection<Generic>)!.Count, lengthMethod);
            AddMethodTranslator(() => default(IReadOnlyCollection<Generic>)!.Count, lengthMethod);
            AddMethodTranslator(() => "".Length, lengthMethod);
            AddMethodTranslator(() => Enums.GetNames<Generic>(), new EnumGetNamesMethodTranslator());
            var identityTranslator = new GenericMethodCompiler(a => a[1]);
            AddMethodTranslator(typeof(BoxingUtils), "Box", identityTranslator, new [] { typeof(bool) });
            AddMethodTranslator(typeof(BoxingUtils), "Box", identityTranslator, new [] { typeof(bool?) });
            AddMethodTranslator(typeof(BoxingUtils), "Box", identityTranslator, new [] { typeof(int) });
            AddMethodTranslator(typeof(BoxingUtils), "Box", identityTranslator, new [] { typeof(int?) });

            JsExpression listGetIndexer(JsExpression[] args, MethodInfo method) =>
                BuildIndexer(args[0], args[1], method.DeclaringType!.GetProperty("Item"));
            JsExpression listSetIndexer(JsExpression[] args, MethodInfo method) =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("setItem").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1], args[2]);
            JsExpression arrayElementSetter(JsExpression[] args, MethodInfo method) =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("setItem").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[2], args[1]);
            JsExpression dictionaryGetIndexer(JsExpression[] args, MethodInfo method) =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("dictionary").Member("getItem").Invoke(args[0], args[1]);
            JsExpression dictionarySetIndexer(JsExpression[] args, MethodInfo method) =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("dictionary").Member("setItem").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1], args[2]);

            AddMethodTranslator(typeof(IList), "get_Item", new GenericMethodCompiler(listGetIndexer));
            AddMethodTranslator(typeof(IList<>), "get_Item", new GenericMethodCompiler(listGetIndexer));
            AddMethodTranslator(typeof(List<>), "get_Item", new GenericMethodCompiler(listGetIndexer));
            AddMethodTranslator(typeof(IList), "set_Item", new GenericMethodCompiler(listSetIndexer));
            AddMethodTranslator(typeof(IList<>), "set_Item", new GenericMethodCompiler(listSetIndexer));
            AddMethodTranslator(typeof(List<>), "set_Item", new GenericMethodCompiler(listSetIndexer));
            AddMethodTranslator(typeof(IReadOnlyList<>), "get_Item", new GenericMethodCompiler(listGetIndexer));
            AddMethodTranslator(typeof(Dictionary<,>), "get_Item", new GenericMethodCompiler(dictionaryGetIndexer));
            AddMethodTranslator(typeof(IDictionary<,>), "get_Item", new GenericMethodCompiler(dictionaryGetIndexer));
            AddMethodTranslator(typeof(Dictionary<,>), "set_Item", new GenericMethodCompiler(dictionarySetIndexer));
            AddMethodTranslator(typeof(IDictionary<,>), "set_Item", new GenericMethodCompiler(dictionarySetIndexer));
            AddMethodTranslator(typeof(IReadOnlyDictionary<,>), "get_Item", new GenericMethodCompiler(dictionaryGetIndexer));
            AddMethodTranslator(typeof(Array).GetMethod(nameof(Array.SetValue), new[] { typeof(object), typeof(int) }), new GenericMethodCompiler(arrayElementSetter));
            AddPropertyGetterTranslator(typeof(Nullable<>), "Value", new GenericMethodCompiler((JsExpression[] args, MethodInfo method) => args[0]));
            AddPropertyGetterTranslator(typeof(Nullable<>), "HasValue",
                new GenericMethodCompiler(args => new JsBinaryExpression(args[0], BinaryOperatorType.NotEqual, new JsLiteral(null))));

            JsBindingApi.RegisterJavascriptTranslations(this);
            BindingApi.RegisterJavascriptTranslations(this);
            BindingPageInfo.RegisterJavascriptTranslations(this);
            BindingCollectionInfo.RegisterJavascriptTranslations(this);

            AddPropertyGetterTranslator(typeof(Task<>), "Result", new GenericMethodCompiler(args => FunctionalExtensions.ApplyAction(args[0], a => a.RemoveAnnotations(typeof(ViewModelInfoAnnotation)))));
            AddPropertyGetterTranslator(typeof(Task), "CompletedTask", new GenericMethodCompiler(_ => new JsIdentifierExpression("undefined")));
            AddMethodTranslator(typeof(Task), "FromResult", new GenericMethodCompiler(args => args[1]));

            AddMethodTranslator(typeof(DotvvmBindableObject).GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(m => m.Name == "GetValue" && !m.ContainsGenericParameters), new GenericMethodCompiler(
                args => {
                    var dotvvmProperty = ((DotvvmProperty)((JsLiteral)args[1]).Value!);
                    return JavascriptTranslationVisitor.TranslateViewModelProperty(args[0], VMPropertyInfoAnnotation.FromDotvvmProperty(dotvvmProperty), name: dotvvmProperty.Name);
                }
            ));

            AddMethodTranslator(typeof(DotvvmBindableObject).GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(m => m.Name == "SetValueToSource" && !m.ContainsGenericParameters), new GenericMethodCompiler(
                args => {
                    var dotvvmProperty = ((DotvvmProperty)((JsLiteral)args[1]).Value!);
                    var p = JavascriptTranslationVisitor.TranslateViewModelProperty(args[0], VMPropertyInfoAnnotation.FromDotvvmProperty(dotvvmProperty), name: dotvvmProperty.Name);
                    return new JsAssignmentExpression(p, args[2]);
                }
            ));

            AddMethodTranslator(typeof(WebUtility), nameof(WebUtility.UrlEncode), translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("encodeURIComponent").Invoke(args[1])));
            AddMethodTranslator(typeof(WebUtility), nameof(WebUtility.UrlDecode), translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("decodeURIComponent").Invoke(args[1])));

            AddDefaultToStringTranslations();
            AddDefaultStringTranslations();
            AddDefaultEnumerableTranslations();
            AddDefaultDictionaryTranslations();
            AddDefaultListTranslations();
            AddDefaultMathTranslations();
            AddDefaultDateTimeTranslations();
            AddDefaultConvertTranslations();
        }

        private void AddDefaultToStringTranslations()
        {
            AddMethodTranslator(typeof(object), "ToString", new PrimitiveToStringTranslator(), 0);
            AddMethodTranslator(typeof(Convert), "ToString", new PrimitiveToStringTranslator(), 1, true);
            AddMethodTranslator(typeof(Enums), "ToEnumString", parameterCount: 1, translator: new GenericMethodCompiler(
                args => args[1]
            ), allowMultipleMethods: true);

            AddMethodTranslator(typeof(DateTime).GetMethod("ToString", Type.EmptyTypes), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(typeof(DateTime).GetMethod("ToString", new[] { typeof(string) }), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(typeof(DateTime?).GetMethod("ToString", Type.EmptyTypes), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(typeof(DateOnly).GetMethod(nameof(DateOnly.ToString), Type.EmptyTypes), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateOnlyToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(typeof(DateOnly?).GetMethod(nameof(Nullable<DateOnly>.ToString), Type.EmptyTypes), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateOnlyToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(typeof(DateOnly).GetMethod(nameof(DateOnly.ToString), new[] { typeof(string) }), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateOnlyToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(typeof(TimeOnly).GetMethod(nameof(TimeOnly.ToString), Type.EmptyTypes), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingTimeOnlyToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(typeof(TimeOnly?).GetMethod(nameof(Nullable<TimeOnly>.ToString), Type.EmptyTypes), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingTimeOnlyToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(typeof(TimeOnly).GetMethod(nameof(TimeOnly.ToString), new[] { typeof(string) }), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingTimeOnlyToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));

            foreach (var num in ReflectionUtils.GetNumericTypes().Except(new[] { typeof(char) }))
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
        }

        private void AddDefaultStringTranslations()
        {
            // TODO: string.Format could be two-way
            AddMethodTranslator(typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object) }), translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("format").Invoke(args[1], args[2])));
            AddMethodTranslator(typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object), typeof(object) }), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("format").Invoke(args[1], args[2], args[3])));
            AddMethodTranslator(typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object), typeof(object), typeof(object) }), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("format").Invoke(args[1], args[2], args[3], args[4])));
            AddMethodTranslator(typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object[]) }), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("format").Invoke(args[1], args[2])));

            AddMethodTranslator(typeof(string), nameof(string.IndexOf), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => a[0].Member("indexOf").Invoke(a[1])));
            AddMethodTranslator(typeof(string), nameof(string.IndexOf), parameters: new[] { typeof(string), typeof(StringComparison) }, translator: new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("indexOf").Invoke(a[0], new JsLiteral(0), a[1], a[2])));
            AddMethodTranslator(typeof(string), nameof(string.IndexOf), parameters: new[] { typeof(string), typeof(int) }, translator: new GenericMethodCompiler(
                a => a[0].Member("indexOf").Invoke(a[1], a[2])));
            AddMethodTranslator(typeof(string), nameof(string.IndexOf), parameters: new[] { typeof(string), typeof(int), typeof(StringComparison) }, translator: new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("indexOf").Invoke(a[0], a[2], a[1], a[3])));
            AddMethodTranslator(typeof(string), nameof(string.LastIndexOf), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => a[0].Member("lastIndexOf").Invoke(a[1])));
            AddMethodTranslator(typeof(string), nameof(string.LastIndexOf), parameters: new[] { typeof(string), typeof(StringComparison) }, translator: new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("lastIndexOf").Invoke(a[0], new JsLiteral(0), a[1], a[2])));
            AddMethodTranslator(typeof(string), nameof(string.LastIndexOf), parameters: new[] { typeof(string), typeof(int) }, translator: new GenericMethodCompiler(
                a => a[0].Member("lastIndexOf").Invoke(a[1], a[2])));
            AddMethodTranslator(typeof(string), nameof(string.LastIndexOf), parameters: new[] { typeof(string), typeof(int), typeof(StringComparison) }, translator: new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("lastIndexOf").Invoke(a[0], a[2], a[1], a[3])));
            AddMethodTranslator(typeof(string), nameof(string.ToUpper), parameterCount: 0, translator: new GenericMethodCompiler(
                a => a[0].Member("toLocaleUpperCase").Invoke()));
            AddMethodTranslator(typeof(string), nameof(string.ToLower), parameterCount: 0, translator: new GenericMethodCompiler(
                a => a[0].Member("toLocaleLowerCase").Invoke()));
            AddMethodTranslator(typeof(string), nameof(string.ToUpperInvariant), parameterCount: 0, translator: new GenericMethodCompiler(
                a => a[0].Member("toUpperCase").Invoke()));
            AddMethodTranslator(typeof(string), nameof(string.ToLowerInvariant), parameterCount: 0, translator: new GenericMethodCompiler(
                a => a[0].Member("toLowerCase").Invoke()));
            AddMethodTranslator(typeof(string), nameof(string.StartsWith), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => a[0].Member("startsWith").Invoke(a[1])));
            AddMethodTranslator(typeof(string), nameof(string.StartsWith), parameters: new[] { typeof(string), typeof(StringComparison) }, translator: new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("startsWith").Invoke(a[0], a[1], a[2])));
            AddMethodTranslator(typeof(string), nameof(string.EndsWith), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => a[0].Member("endsWith").Invoke(a[1])));
            AddMethodTranslator(typeof(string), nameof(string.EndsWith), parameters: new[] { typeof(string), typeof(StringComparison) }, translator: new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("endsWith").Invoke(a[0], a[1], a[2])));
            AddMethodTranslator(typeof(string), nameof(string.IsNullOrEmpty), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => new JsBinaryExpression(a[1].Member("length", optional: true), BinaryOperatorType.Greater, new JsLiteral(0)).Unary(UnaryOperatorType.LogicalNot)
            ));
            AddMethodTranslator(typeof(string), nameof(string.IsNullOrWhiteSpace), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => new JsBinaryExpression(a[1].Member("trim", optional: true).Invoke().Member("length"), BinaryOperatorType.Greater, new JsLiteral(0)).Unary(UnaryOperatorType.LogicalNot)));

            var joinStringArrayMethod = typeof(string).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(string.Join) && m.GetParameters().Length == 2 && m.GetParameters().Last().ParameterType == typeof(string[]) && m.GetParameters().First().ParameterType == typeof(string)).Single();
            AddMethodTranslator(joinStringArrayMethod, translator: new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("join").Invoke(args[2], args[1])));
            var joinStringEnumerableMethod = typeof(string).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(string.Join) && m.GetParameters().Length == 2 && m.GetParameters().Last().ParameterType == typeof(IEnumerable<string>) && m.GetParameters().First().ParameterType == typeof(string)).Single();
            AddMethodTranslator(joinStringEnumerableMethod, translator: new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("join").Invoke(args[2], args[1])));

            AddMethodTranslator(typeof(string), nameof(string.Replace), parameters: new[] { typeof(string), typeof(string) }, translator: new GenericMethodCompiler(
                args => args[0].Member("split").Invoke(args[1]).Member("join").Invoke(args[2])));

            AddMethodTranslator(typeof(string), nameof(string.Contains), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => a[0].Member("includes").Invoke(a[1])));

            AddFrameworkDependentSplitMehtodTranslations();
            AddFrameworkDependentContainsMehtodTranslations();


        }

        private void AddFrameworkDependentContainsMehtodTranslations()
        {
            var containsMehtod = typeof(string).GetMethods(BindingFlags.Public | BindingFlags.Instance).SingleOrDefault(m => m.Name == nameof(string.Contains)
                && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(string) && m.GetParameters()[1].ParameterType == typeof(StringComparison));
            if (containsMehtod != null)
            {
                AddMethodTranslator(typeof(string), nameof(string.Contains), parameters: new[] { typeof(string), typeof(StringComparison) }, translator: new GenericMethodCompiler(
                   a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("contains").Invoke(a[0], a[1], a[2])));
            }
            else
            {
                // Some overloads are not available in .NET Framework, therefore we substitute some with custom extensions
                AddMethodTranslator(typeof(NetFrameworkExtensions), nameof(NetFrameworkExtensions.Contains), parameters: new[] { typeof(string), typeof(string), typeof(StringComparison) }, translator: new GenericMethodCompiler(
                    a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("contains").Invoke(a[1], a[2], a[3])));
            }
        }

        private void AddFrameworkDependentSplitMehtodTranslations()
        {
            var splitMethod = typeof(string).GetMethods(BindingFlags.Public | BindingFlags.Instance).SingleOrDefault(m => m.Name == nameof(string.Split)
                && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(string) && m.GetParameters()[1].ParameterType == typeof(StringSplitOptions));
            var isNetFramework = splitMethod == null;
            if (isNetFramework)
            {
                var genericExtensionSplitCompiler = new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("split").Invoke(args[1], args[2], args[3]));
                AddMethodTranslator(typeof(NetFrameworkExtensions), nameof(NetFrameworkExtensions.Split), parameters: new[] { typeof(string), typeof(char), typeof(StringSplitOptions) },
                    translator: genericExtensionSplitCompiler);
                AddMethodTranslator(typeof(NetFrameworkExtensions), nameof(NetFrameworkExtensions.Split), parameters: new[] { typeof(string), typeof(string), typeof(StringSplitOptions) },
                    translator: genericExtensionSplitCompiler);
            }
            else
            {
                var genericSplitCompiler = new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("split").Invoke(args[0], args[1], args[2]));
                AddMethodTranslator(typeof(string), nameof(string.Split), parameters: new[] { typeof(char), typeof(StringSplitOptions) }, translator: genericSplitCompiler);
                AddMethodTranslator(typeof(string), nameof(string.Split), parameters: new[] { typeof(string), typeof(StringSplitOptions) }, translator: genericSplitCompiler);
            }
            AddMethodTranslator(typeof(string), nameof(NetFrameworkExtensions.Split), parameters: new[] { typeof(char[]) },
                translator: new GenericMethodCompiler(args => args[0].Member("split").Invoke(args[1])));

            if (isNetFramework)
            {
                JsExpression TakeFirstElement(JsExpression array)
                    => new JsIndexerExpression(array, new JsLiteral(0));

                AddMethodTranslator(typeof(string), nameof(string.Trim), parameters: new[] { typeof(char[]) }, translator: new GenericMethodCompiler(
                    a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimEnd").Invoke(
                        new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimStart").Invoke(a[0], TakeFirstElement(a[1])), TakeFirstElement(a[1].Clone()))));
                AddMethodTranslator(typeof(NetFrameworkExtensions), nameof(NetFrameworkExtensions.TrimStart), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                    a => a[1].Member("trimStart").Invoke()));
                AddMethodTranslator(typeof(string), nameof(string.TrimStart), parameters: new[] { typeof(char[]) }, translator: new GenericMethodCompiler(
                    a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimStart").Invoke(a[0], TakeFirstElement(a[1]))));
                AddMethodTranslator(typeof(NetFrameworkExtensions), nameof(NetFrameworkExtensions.TrimEnd), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                    a => a[1].Member("trimEnd").Invoke()));
                AddMethodTranslator(typeof(string), nameof(string.TrimEnd), parameters: new[] { typeof(char[]) }, translator: new GenericMethodCompiler(
                    a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimEnd").Invoke(a[0], TakeFirstElement(a[1]))));
            }
            else
            {
                AddMethodTranslator(typeof(string), nameof(string.Trim), parameters: new[] { typeof(char) }, translator: new GenericMethodCompiler(
                    a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimEnd").Invoke(
                        new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimStart").Invoke(a[0], a[1]), a[1].Clone())));
                AddMethodTranslator(typeof(string), nameof(string.TrimStart), parameterCount: 0, translator: new GenericMethodCompiler(
                    a => a[0].Member("trimStart").Invoke()));
                AddMethodTranslator(typeof(string), nameof(string.TrimStart), parameters: new[] { typeof(char) }, translator: new GenericMethodCompiler(
                    a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimStart").Invoke(a[0], a[1])));
                AddMethodTranslator(typeof(string), nameof(string.TrimEnd), parameterCount: 0, translator: new GenericMethodCompiler(
                    a => a[0].Member("trimEnd").Invoke()));
                AddMethodTranslator(typeof(string), nameof(string.TrimEnd), parameters: new[] { typeof(char) }, translator: new GenericMethodCompiler(
                    a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimEnd").Invoke(a[0], a[1])));
            }

            AddMethodTranslator(typeof(string), nameof(string.Trim), parameterCount: 0, translator: new GenericMethodCompiler(
                a => a[0].Member("trim").Invoke()));
            AddMethodTranslator(typeof(string), nameof(string.PadLeft), parameterCount: 1, translator: new GenericMethodCompiler(
                a => a[0].Member("padStart").Invoke(a[1])));
            AddMethodTranslator(typeof(string), nameof(string.PadLeft), parameterCount: 2, translator: new GenericMethodCompiler(
                a => a[0].Member("padStart").Invoke(a[1], a[2])));
            AddMethodTranslator(typeof(string), nameof(string.PadRight), parameterCount: 1, translator: new GenericMethodCompiler(
                a => a[0].Member("padEnd").Invoke(a[1])));
            AddMethodTranslator(typeof(string), nameof(string.PadRight), parameterCount: 2, translator: new GenericMethodCompiler(
                a => a[0].Member("padEnd").Invoke(a[1], a[2])));
        }

        private void AddDefaultMathTranslations()
        {
            AddMethodTranslator(typeof(Math), nameof(Math.Abs), parameters: new[] { typeof(int) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("abs").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Abs), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("abs").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Acos), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("acos").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Asin), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("asin").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Atan), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("atan").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Atan2), parameters: new[] { typeof(double), typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("atan2").Invoke(args[1], args[2])));

            AddMethodTranslator(typeof(Math), nameof(Math.Ceiling), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("ceil").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Cos), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("cos").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Cosh), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("cosh").Invoke(args[1])));

            AddMethodTranslator(typeof(Math), nameof(Math.Exp), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("exp").Invoke(args[1])));

            AddMethodTranslator(typeof(Math), nameof(Math.Floor), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("floor").Invoke(args[1])));

            AddMethodTranslator(typeof(Math), nameof(Math.Log), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("log").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Log10), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("log10").Invoke(args[1])));

            AddMethodTranslator(typeof(Math), nameof(Math.Max), parameters: new[] { typeof(int), typeof(int) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("max").Invoke(args[1], args[2])));
            AddMethodTranslator(typeof(Math), nameof(Math.Max), parameters: new[] { typeof(double), typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("max").Invoke(args[1], args[2])));
            AddMethodTranslator(typeof(Math), nameof(Math.Min), parameters: new[] { typeof(int), typeof(int) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("min").Invoke(args[1], args[2])));
            AddMethodTranslator(typeof(Math), nameof(Math.Min), parameters: new[] { typeof(double), typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("min").Invoke(args[1], args[2])));

            AddMethodTranslator(typeof(Math), nameof(Math.Pow), parameters: new[] { typeof(double), typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("pow").Invoke(args[1], args[2])));

            AddMethodTranslator(typeof(Math), nameof(Math.Round), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("round").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Round), parameters: new[] { typeof(double), typeof(int) }, translator: new GenericMethodCompiler(
                args => args[1].Member("toFixed").Invoke(args[2])));

            AddMethodTranslator(typeof(Math), nameof(Math.Sign), parameters: new[] { typeof(int) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("sign").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Sign), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("sign").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Sin), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("sin").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Sinh), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("sinh").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Sqrt), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("sqrt").Invoke(args[1])));

            AddMethodTranslator(typeof(Math), nameof(Math.Tan), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("tan").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Tanh), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("tanh").Invoke(args[1])));
            AddMethodTranslator(typeof(Math), nameof(Math.Truncate), parameters: new[] { typeof(double) }, translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("trunc").Invoke(args[1])));
        }

        private bool EnsureIsComparableInJavascript(MethodInfo method, Type type)
        {
            if (!ReflectionUtils.IsPrimitiveType(type))
                throw new DotvvmCompilationException($"Cannot translate invocation of method \"{method.Name}\" to JavaScript. Comparison of non-primitive types is not supported.");

            return true;
        }

        private void AddDefaultEnumerableTranslations()
        {
            var returnTrueFunc = new JsArrowFunctionExpression(Enumerable.Empty<JsIdentifier>(), new JsLiteral(true));
            var selectIdentityFunc = new JsArrowFunctionExpression(new[] { new JsIdentifier("arg") },
                new JsIdentifierExpression("ko").Member("unwrap").Invoke(new JsIdentifierExpression("arg")));

            bool IsDelegateReturnTypeEnum(Type type)
                => type.GetGenericArguments().Last().IsEnum;

            string GetDelegateReturnTypeHash(Type type)
                => type.GetGenericArguments().Last().GetTypeHash();

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.All), parameterCount: 2, translator: new GenericMethodCompiler(args => args[1].Member("every").Invoke(args[2])));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Any), parameterCount: 1, translator: new GenericMethodCompiler(args => args[1].Member("some").Invoke(returnTrueFunc.Clone())));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Any), parameterCount: 2, translator: new GenericMethodCompiler(args => args[1].Member("some").Invoke(args[2])));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Concat), parameterCount: 2, translator: new GenericMethodCompiler(args => args[1].Member("concat").Invoke(args[2])));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Count), parameterCount: 1, translator: new GenericMethodCompiler(args => args[1].Member("length")));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Distinct), parameterCount: 1,
                translator: new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("distinct").Invoke(args[1]),
                check: (method, target, arguments) => EnsureIsComparableInJavascript(method, ReflectionUtils.GetEnumerableType(arguments.First().Type).NotNull())));

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.ElementAt), parameterCount: 2, parameterFilter: p => p[1].ParameterType == typeof(int),
                translator: new GenericMethodCompiler((args, method) => BuildIndexer(args[1], args[2], method)));

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.FirstOrDefault), parameterCount: 1, translator: new GenericMethodCompiler((args, m) =>
                new JsIndexerExpression(args[1], new JsLiteral(0))
                    .WithAnnotation(new VMPropertyInfoAnnotation(m.ReturnType)).WithAnnotation(MayBeNullAnnotation.Instance)));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.FirstOrDefault), parameterCount: 2, parameterFilter: p => p[1].ParameterType.IsGenericType && p[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>), translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("firstOrDefault").Invoke(args[1], args[2]).WithAnnotation(MayBeNullAnnotation.Instance)));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.LastOrDefault), parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("lastOrDefault").Invoke(args[1], returnTrueFunc.Clone()).WithAnnotation(MayBeNullAnnotation.Instance)));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.LastOrDefault), parameterCount: 2, parameterFilter: p => p[1].ParameterType.IsGenericType && p[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>), translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("lastOrDefault").Invoke(args[1], args[2]).WithAnnotation(MayBeNullAnnotation.Instance)));

            foreach (var type in new[] { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) })
            {
                AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Max), parameters: new[] { typeof(IEnumerable<>).MakeGenericType(type) }, translator: new GenericMethodCompiler(args =>
                    new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("max").Invoke(args[1], selectIdentityFunc.Clone(), new JsLiteral(!type.IsNullable()))));
                AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Max), parameterCount: 2, parameterFilter: p => p[1].ParameterType.GetGenericArguments().Last() == type, translator: new GenericMethodCompiler(args =>
                    new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("max").Invoke(args[1], args[2], new JsLiteral(!type.IsNullable()))));

                AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Min), parameters: new[] { typeof(IEnumerable<>).MakeGenericType(type) }, translator: new GenericMethodCompiler(args =>
                    new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("min").Invoke(args[1], selectIdentityFunc.Clone(), new JsLiteral(!type.IsNullable()))));
                AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Min), parameterCount: 2, parameterFilter: p => p[1].ParameterType.GetGenericArguments().Last() == type, translator: new GenericMethodCompiler(args =>
                    new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("min").Invoke(args[1], args[2], new JsLiteral(!type.IsNullable()))));
            }

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.OrderBy), parameterCount: 2,
                translator: new GenericMethodCompiler((jArgs, dArgs) => new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("orderBy")
                    .Invoke(jArgs[1], jArgs[2], new JsLiteral((IsDelegateReturnTypeEnum(dArgs.Last().Type)) ? GetDelegateReturnTypeHash(dArgs.Last().Type) : null)),
                check: (method, _, arguments) => EnsureIsComparableInJavascript(method, arguments.Last().Type.GetGenericArguments().Last())));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.OrderByDescending), parameterCount: 2,
                translator: new GenericMethodCompiler((jArgs, dArgs) => new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("orderByDesc")
                    .Invoke(jArgs[1], jArgs[2], new JsLiteral((IsDelegateReturnTypeEnum(dArgs.Last().Type)) ? GetDelegateReturnTypeHash(dArgs.Last().Type) : null)),
                check: (method, _, arguments) => EnsureIsComparableInJavascript(method, arguments.Last().Type.GetGenericArguments().Last())));

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Select), parameterCount: 2, parameterFilter: p => p[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>),
                translator: new GenericMethodCompiler(args => args[1].Member("map").Invoke(args[2])));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Skip), parameterCount: 2, translator: new GenericMethodCompiler(args => args[1].Member("slice").Invoke(args[2])));

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Take), parameterCount: 2, parameterFilter: p => p[1].ParameterType == typeof(int), translator: new GenericMethodCompiler(args =>
                args[1].Member("slice").Invoke(new JsLiteral(0), args[2])));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.ToArray), parameterCount: 1, translator: new GenericMethodCompiler(args => args[1]));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.ToList), parameterCount: 1, translator: new GenericMethodCompiler(args => args[1]));

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Where), parameterCount: 2, parameterFilter: p => p[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>),
                translator: new GenericMethodCompiler(args => args[1].Member("filter").Invoke(args[2])));
        }

        private void AddDefaultDictionaryTranslations()
        {
            AddMethodTranslator(typeof(Dictionary<,>), "Clear", parameterCount: 0, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("dictionary").Member("clear").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))));
            var containsKey = new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("dictionary").Member("containsKey").Invoke(args[0], args[1]));
            AddMethodTranslator(typeof(IDictionary<,>), "ContainsKey", parameterCount: 1, translator: containsKey);
            AddMethodTranslator(typeof(IReadOnlyDictionary<,>), "ContainsKey", parameterCount: 1, translator: containsKey);
            AddMethodTranslator(typeof(IDictionary<,>), "Remove", parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("dictionary").Member("remove").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])));
        }

        private void AddDefaultListTranslations()
        {
            AddMethodTranslator(typeof(List<>), "Add", parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("add").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])));
            AddMethodTranslator(typeof(List<>), "AddRange", parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("addRange").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])));
            AddMethodTranslator(typeof(List<>), "Clear", parameterCount: 0, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("clear").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))));
            AddMethodTranslator(typeof(List<>), "Insert", parameterCount: 2, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("insert").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1], args[2])));
            AddMethodTranslator(typeof(List<>), "InsertRange", parameterCount: 2, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("insertRange").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1], args[2])));
            AddMethodTranslator(typeof(List<>), "RemoveAt", parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("removeAt").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])));
            AddMethodTranslator(typeof(List<>), "RemoveAll", parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("removeAll").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])));
            AddMethodTranslator(typeof(List<>), "RemoveRange", parameterCount: 2, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("removeRange").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1], args[2])));
            AddMethodTranslator(typeof(List<>), "Reverse", parameterCount: 0, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("reverse").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))));
            AddMethodTranslator(typeof(List<>), "AsReadOnly", parameterCount: 0, translator: new GenericMethodCompiler(args => args[0]));
            AddMethodTranslator(
               typeof(List<>), "Contains",
               parameterCount: 1,
               translator: new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("contains").Invoke(args[0], args[1]).WithAnnotation(MayBeNullAnnotation.Instance),
               check: (method, target, arguments) => target is not null && EnsureIsComparableInJavascript(method, ReflectionUtils.GetEnumerableType(target.Type).NotNull())));

            // DotVVM list extensions:
            AddMethodTranslator(typeof(ListExtensions), "AddOrUpdate", parameterCount: 4, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("addOrUpdate").Invoke(args[1].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[2], args[3], args[4])));
            AddMethodTranslator(typeof(ListExtensions), "RemoveFirst", parameterCount: 2, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("removeFirst").Invoke(args[1].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[2])));
            AddMethodTranslator(typeof(ListExtensions), "RemoveLast", parameterCount: 2, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("removeLast").Invoke(args[1].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[2])));
        }

        private void AddDefaultDateTimeTranslations()
        {
            JsExpression IncrementExpression(JsExpression left, int value)
                => new JsBinaryExpression(left, BinaryOperatorType.Plus, new JsLiteral(value));

            AddPropertyGetterTranslator(typeof(DateTime), nameof(DateTime.Year), translator: new GenericMethodCompiler(args =>
                new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getFullYear").Invoke()));
            AddPropertyGetterTranslator(typeof(DateTime), nameof(DateTime.Month), translator: new GenericMethodCompiler(args =>
                IncrementExpression(new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getMonth").Invoke(), 1)));
            AddPropertyGetterTranslator(typeof(DateTime), nameof(DateTime.Day), translator: new GenericMethodCompiler(args =>
                new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getDate").Invoke()));
            AddPropertyGetterTranslator(typeof(DateTime), nameof(DateTime.Hour), translator: new GenericMethodCompiler(args =>
                new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getHours").Invoke()));
            AddPropertyGetterTranslator(typeof(DateTime), nameof(DateTime.Minute), translator: new GenericMethodCompiler(args =>
                new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getMinutes").Invoke()));
            AddPropertyGetterTranslator(typeof(DateTime), nameof(DateTime.Second), translator: new GenericMethodCompiler(args =>
                new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getSeconds").Invoke()));
            AddPropertyGetterTranslator(typeof(DateTime), nameof(DateTime.Millisecond), translator: new GenericMethodCompiler(args =>
                new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getMilliseconds").Invoke()));

            AddMethodTranslator(typeof(DateTimeExtensions), nameof(DateTimeExtensions.ToBrowserLocalTime), parameterCount: 1, parameterFilter: p => p[0].ParameterType == typeof(DateTime), translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("dateTime").Member("toBrowserLocalTime").Invoke(args[1].WithAnnotation(ShouldBeObservableAnnotation.Instance)).WithAnnotation(ResultIsObservableAnnotation.Instance)));
            AddMethodTranslator(typeof(DateTimeExtensions), nameof(DateTimeExtensions.ToBrowserLocalTime), parameterCount: 1, parameterFilter: p => p[0].ParameterType == typeof(DateTime?), translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("dateTime").Member("toBrowserLocalTime").Invoke(args[1].WithAnnotation(ShouldBeObservableAnnotation.Instance)).WithAnnotation(ResultIsObservableAnnotation.Instance)));
        }

        private void AddDefaultConvertTranslations()
        {
            // Convert.ToDouble, ToSingle, ToDecimal - all of these are represented as double in JS, we only sometimes need them for correct overload resolution

            // for integer types, we do the same, but also call Math.round

            foreach (var m in typeof(Convert)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name is "ToDouble" or "ToSingle" or "ToDecimal" or "ToInt32" or "ToUInt32" or "ToInt16" or "ToUInt16" or "ToByte" or "ToSByte" or "ToInt64" or "ToUInt64"))
            {
                var p = m.GetParameters();
                if (p.Length != 1)
                    continue;
                var isFloating = m.Name is "ToDouble" or "ToSingle" or "ToDecimal";
                JsExpression wrapInRound(JsExpression a) =>
                    isFloating ? a : new JsIdentifierExpression("Math").Member("round").Invoke(a);
                if (p[0].ParameterType == typeof(char))
                {
                    // Convert char to number
                    AddMethodTranslator(m, translator: new GenericMethodCompiler(args => args[1].Member("charCodeAt").Invoke(new JsLiteral(0))));
                }
                else if (p[0].ParameterType.IsNumericType())
                {
                    AddMethodTranslator(m, translator: new GenericMethodCompiler(args => wrapInRound(args[1])));
                }
                else if (p[0].ParameterType == typeof(string) || p[0].ParameterType == typeof(bool))
                {
                    AddMethodTranslator(m, translator: new GenericMethodCompiler(args => wrapInRound(new JsIdentifierExpression("Number").Invoke(args[1]))));
                }
            }

            foreach (var m in typeof(Convert)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name == "ToBoolean"))
            {
                var p = m.GetParameters();
                if (p.Length != 1)
                    continue;
                if (p[0].ParameterType.IsNumericType() && p[0].ParameterType != typeof(char))
                {
                    AddMethodTranslator(m, translator: new GenericMethodCompiler(args => new JsIdentifierExpression("Boolean").Invoke(args[1])));
                }
            }
        }
        public JsExpression? TryTranslateCall(LazyTranslatedExpression? context, LazyTranslatedExpression[] args, MethodInfo method)
        {
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

            if (!method.DeclaringType!.IsInterface)
            {
                // attempt to match a translation defined on an interface. For example Dictionary`2.ContainsKey should match the IDictionary`2.ContainsKey translation
                foreach (var iface in method.DeclaringType.GetInterfaces())
                {
                    if (Interfaces.Contains(iface) || iface.IsConstructedGenericType && Interfaces.Contains(iface.GetGenericTypeDefinition()))
                    {
                        var map = method.DeclaringType.GetInterfaceMap(iface);
                        var imIndex = Array.IndexOf(map.TargetMethods, method);
                        if (imIndex >= 0 && TryTranslateCall(context, args, map.InterfaceMethods[imIndex]) is JsExpression result)
                            return result;
                    }
                }
            }
            if (method.DeclaringType.IsGenericType && !method.DeclaringType.IsGenericTypeDefinition)
            {
                var genericType = method.DeclaringType.GetGenericTypeDefinition();
                var m2 = genericType.GetMethod(method.Name,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                    binder: null, types: method.GetParameters().Select(p => p.ParameterType).ToArray(), modifiers: null);

                if (m2 == null)
                {
                    var parameters = method.GetParameters();
                    foreach (var m in genericType.GetMethods().Where(m => m.Name == method.Name))
                    {
                        var genParameters = m.GetParameters();
                        if (parameters.Length != genParameters.Length)
                            continue;

                        var isMatch = true;
                        for (var index = 0; index < parameters.Length; index++)
                        {
                            // At this point we already know that there is no non-generic method that matches provided parameters
                            var concrete = parameters[index].ParameterType;
                            var generic = (!genParameters[index].ParameterType.IsGenericType) ?
                                genParameters[index].ParameterType : genParameters[index].ParameterType.GetGenericTypeDefinition();

                            if (genParameters[index].ParameterType.IsGenericParameter)
                            {
                                continue;
                            }
                            else if (genParameters[index].ParameterType.IsGenericType)
                            {
                                if (!ReflectionUtils.IsAssignableToGenericType(concrete, generic, out var _))
                                {
                                    isMatch = false;
                                    break;
                                }

                                continue;
                            }
                            else if (genParameters[index].ParameterType != parameters[index].ParameterType)
                            {
                                isMatch = false;
                                break;
                            }
                        }

                        if (isMatch)
                        {
                            m2 = m;
                            break;
                        }
                    }
                }

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
