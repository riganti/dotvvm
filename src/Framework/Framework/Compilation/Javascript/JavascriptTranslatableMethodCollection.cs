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
            CheckNotAccidentalDefinition(method);
            AddMethodTranslator(method, translator);
        }
        public void AddMethodTranslator(Expression<Action> methodCall, IJavascriptMethodTranslator translator)
        {
            var method = (MethodInfo)MethodFindingHelper.GetMethodFromExpression(methodCall);
            CheckNotAccidentalDefinition(method);
            AddMethodTranslator(method, translator);
        }

        private void CheckNotAccidentalDefinition(MethodBase m)
        {
            if (m.DeclaringType == typeof(object))
                throw new NotSupportedException($"Method {m} declared on System.Object cannot be registered using this overload (to prevent accidental registration of all ToString method, etc). The overload taking MethodInfo doesn't contain this check.");
        }

        public void AddPropertyTranslator<T>(Expression<Func<T>> propertyAccess, IJavascriptMethodTranslator? getter, IJavascriptMethodTranslator? setter = null)
        {
            var property = MethodFindingHelper.GetPropertyFromExpression(propertyAccess);
            if (getter is {})
            {
                if (property.GetMethod is null)
                    throw new NotSupportedException($"Property {property} does not have a getter");
                AddMethodTranslator(property.GetMethod, getter);
            }
            if (setter is {})
            {
                if (property.SetMethod is null)
                    throw new NotSupportedException($"Property {property} does not have a setter");
                AddMethodTranslator(property.SetMethod, setter);
            }
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
            if (!MethodTranslators.TryAdd(method, translator))
            {
                throw new Exception($"Method {ReflectionUtils.FormatMethodInfo(method)} is already registered.");
            }
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
            AddMethodTranslator(() => default(ICollection<Generic.T>)!.Count, lengthMethod);
            AddMethodTranslator(() => default(IReadOnlyCollection<Generic.T>)!.Count, lengthMethod);
            AddMethodTranslator(() => "".Length, lengthMethod);
            AddMethodTranslator(() => Enums.GetNames<Generic.Enum>(), new EnumGetNamesMethodTranslator());
            var identityTranslator = new GenericMethodCompiler(a => a[1]);
            AddMethodTranslator(() => BoxingUtils.Box(default(bool)), identityTranslator);
            AddMethodTranslator(() => BoxingUtils.Box(default(int)), identityTranslator);
            AddMethodTranslator(() => BoxingUtils.Box(default(int?)), identityTranslator);

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

            AddPropertyTranslator(() => default(IList)![0], new GenericMethodCompiler(listGetIndexer), new GenericMethodCompiler(listSetIndexer));
            AddPropertyTranslator(() => default(IList<Generic.T>)![0], new GenericMethodCompiler(listGetIndexer), new GenericMethodCompiler(listSetIndexer));
            AddPropertyTranslator(() => default(List<Generic.T>)![0], new GenericMethodCompiler(listGetIndexer), new GenericMethodCompiler(listSetIndexer));
            AddPropertyTranslator(() => default(IReadOnlyList<Generic.T>)![0], new GenericMethodCompiler(listGetIndexer));
            AddPropertyTranslator(() => default(Dictionary<Generic.T, Generic.T>)![null!], new GenericMethodCompiler(dictionaryGetIndexer), new GenericMethodCompiler(dictionarySetIndexer));
            AddPropertyTranslator(() => default(IDictionary<Generic.T, Generic.T>)![null!], new GenericMethodCompiler(dictionaryGetIndexer), new GenericMethodCompiler(dictionarySetIndexer));
            AddPropertyTranslator(() => default(IReadOnlyDictionary<Generic.T, Generic.T>)![null!], new GenericMethodCompiler(dictionaryGetIndexer));
            AddMethodTranslator(() => default(Array)!.SetValue(null, 1), new GenericMethodCompiler(arrayElementSetter));
            AddPropertyTranslator(() => default(Generic.Struct?)!.Value, new GenericMethodCompiler((JsExpression[] args, MethodInfo method) => args[0]));
            AddPropertyTranslator(() => default(Generic.Struct?)!.HasValue,
                new GenericMethodCompiler(args => new JsBinaryExpression(args[0], BinaryOperatorType.NotEqual, new JsLiteral(null))));

            JsBindingApi.RegisterJavascriptTranslations(this);
            BindingApi.RegisterJavascriptTranslations(this);
            BindingPageInfo.RegisterJavascriptTranslations(this);
            BindingCollectionInfo.RegisterJavascriptTranslations(this);

            AddPropertyTranslator(() => default(Task<Generic.T>)!.Result, new GenericMethodCompiler(args => FunctionalExtensions.ApplyAction(args[0], a => a.RemoveAnnotations(typeof(ViewModelInfoAnnotation)))));
            AddPropertyTranslator(() => Task.CompletedTask, new GenericMethodCompiler(_ => new JsIdentifierExpression("undefined")));
            AddMethodTranslator(() => Task.FromResult(default(Generic.T)), new GenericMethodCompiler(args => args[1]));

            AddMethodTranslator(() => default(DotvvmBindableObject)!.GetValue(default(DotvvmProperty)!, true), new GenericMethodCompiler(
                args => {
                    var dotvvmProperty = ((DotvvmProperty)((JsLiteral)args[1]).Value!);
                    return JavascriptTranslationVisitor.TranslateViewModelProperty(args[0], VMPropertyInfoAnnotation.FromDotvvmProperty(dotvvmProperty), name: dotvvmProperty.Name);
                }
            ));

            AddMethodTranslator(() => default(DotvvmBindableObject)!.SetValueToSource(default(DotvvmProperty)!, null), new GenericMethodCompiler(
                args => {
                    var dotvvmProperty = ((DotvvmProperty)((JsLiteral)args[1]).Value!);
                    var p = JavascriptTranslationVisitor.TranslateViewModelProperty(args[0], VMPropertyInfoAnnotation.FromDotvvmProperty(dotvvmProperty), name: dotvvmProperty.Name);
                    return new JsAssignmentExpression(p, args[2]);
                }
            ));

            AddMethodTranslator(() => WebUtility.UrlEncode(""), translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("encodeURIComponent").Invoke(args[1])));
            AddMethodTranslator(() => WebUtility.UrlDecode(""), translator: new GenericMethodCompiler(
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
            AddMethodTranslator(typeof(object).GetMethod("ToString", Type.EmptyTypes), new PrimitiveToStringTranslator());
            AddMethodTranslator(typeof(Convert), "ToString", new PrimitiveToStringTranslator(), parameterCount: 1, allowMultipleMethods: true);
            AddMethodTranslator(typeof(Enums), "ToEnumString", parameterCount: 1, translator: new GenericMethodCompiler(
                args => args[1]
            ), allowMultipleMethods: true);

            AddMethodTranslator(() => DateTime.Now.ToString(), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(() => DateTime.Now.ToString("fmt"), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(() => default(Nullable<DateTime>).ToString(), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(() => default(DateOnly).ToString(), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateOnlyToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(() => default(DateOnly?).ToString(), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateOnlyToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(() => default(DateOnly).ToString("fmt"), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingDateOnlyToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(() => default(TimeOnly).ToString(), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingTimeOnlyToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(() => default(TimeOnly?).ToString(), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("bindingTimeOnlyToString")
                        .WithAnnotation(new GlobalizeResourceBindingProperty())
                        .Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                        .WithAnnotation(ResultIsObservableAnnotation.Instance)
            ));
            AddMethodTranslator(() => default(TimeOnly).ToString("fmt"), new GenericMethodCompiler(
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
            AddMethodTranslator(() => string.Format("", new object()), translator: new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("format").Invoke(args[1], args[2])));
            AddMethodTranslator(() => string.Format("", new object(), new object()), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("format").Invoke(args[1], args[2], args[3])));
            AddMethodTranslator(() => string.Format("", new object(), new object(), new object()), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("globalize").Member("format").Invoke(args[1], args[2], args[3], args[4])));
            AddMethodTranslator(() => string.Format("", new object[0]), new GenericMethodCompiler(
                args => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("format").Invoke(args[1], args[2])));

            AddMethodTranslator(() => "".IndexOf(""), translator: new GenericMethodCompiler(
                a => a[0].Member("indexOf").Invoke(a[1])));
            AddMethodTranslator(() => "".IndexOf("", StringComparison.Ordinal), translator: new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("indexOf").Invoke(a[0], new JsLiteral(0), a[1], a[2])));
            AddMethodTranslator(() => "".IndexOf("a", 1), translator: new GenericMethodCompiler(
                a => a[0].Member("indexOf").Invoke(a[1], a[2])));
            AddMethodTranslator(() => "".IndexOf("a", 1, StringComparison.Ordinal), translator: new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("indexOf").Invoke(a[0], a[2], a[1], a[3])));
            AddMethodTranslator(() => "".LastIndexOf("a"), translator: new GenericMethodCompiler(
                a => a[0].Member("lastIndexOf").Invoke(a[1])));
            AddMethodTranslator(() => "".LastIndexOf("a", StringComparison.Ordinal), translator: new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("lastIndexOf").Invoke(a[0], new JsLiteral(0), a[1], a[2])));
            AddMethodTranslator(() => "".LastIndexOf("a", 1), translator: new GenericMethodCompiler(
                a => a[0].Member("lastIndexOf").Invoke(a[1], a[2])));
            AddMethodTranslator(() => "".LastIndexOf("a", 1, StringComparison.Ordinal), translator: new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("lastIndexOf").Invoke(a[0], a[2], a[1], a[3])));
            AddMethodTranslator(() => "".ToUpper(), translator: new GenericMethodCompiler(
                a => a[0].Member("toLocaleUpperCase").Invoke()));
            AddMethodTranslator(() => "".ToLower(), translator: new GenericMethodCompiler(
                a => a[0].Member("toLocaleLowerCase").Invoke()));
            AddMethodTranslator(() => "".ToUpperInvariant(), translator: new GenericMethodCompiler(
                a => a[0].Member("toUpperCase").Invoke()));
            AddMethodTranslator(() => "".ToLowerInvariant(), translator: new GenericMethodCompiler(
                a => a[0].Member("toLowerCase").Invoke()));
            AddMethodTranslator(() => "".StartsWith(""), translator: new GenericMethodCompiler(
                a => a[0].Member("startsWith").Invoke(a[1])));
            AddMethodTranslator(() => "".StartsWith("", StringComparison.Ordinal), translator: new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("startsWith").Invoke(a[0], a[1], a[2])));
            AddMethodTranslator(() => "".EndsWith(""), translator: new GenericMethodCompiler(
                a => a[0].Member("endsWith").Invoke(a[1])));
            AddMethodTranslator(() => "".EndsWith("", StringComparison.Ordinal), translator: new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("endsWith").Invoke(a[0], a[1], a[2])));
            AddMethodTranslator(() => string.IsNullOrEmpty(""), translator: new GenericMethodCompiler(
                a => new JsBinaryExpression(a[1].Member("length", optional: true), BinaryOperatorType.Greater, new JsLiteral(0)).Unary(UnaryOperatorType.LogicalNot)
            ));
            AddMethodTranslator(() => string.IsNullOrWhiteSpace(""), translator: new GenericMethodCompiler(
                a => new JsBinaryExpression(a[1].Member("trim", optional: true).Invoke().Member("length"), BinaryOperatorType.Greater, new JsLiteral(0)).Unary(UnaryOperatorType.LogicalNot)));

            AddMethodTranslator(() => string.Join("", new string[0]), translator: new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("join").Invoke(args[2], args[1])));
            AddMethodTranslator(() => string.Join("", Enumerable.Empty<string>()), translator: new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("join").Invoke(args[2], args[1])));

            AddMethodTranslator(() => "".Replace("a", "b"), translator: new GenericMethodCompiler(
                args => args[0].Member("split").Invoke(args[1]).Member("join").Invoke(args[2])));

            AddMethodTranslator(() => "".Contains("a"), translator: new GenericMethodCompiler(
                a => a[0].Member("includes").Invoke(a[1])));

            AddFrameworkDependentSplitMehtodTranslations();
            AddFrameworkDependentContainsMehtodTranslations();
        }

        private void AddFrameworkDependentContainsMehtodTranslations()
        {
#if DotNetCore
            AddMethodTranslator(() => "".Contains("", StringComparison.Ordinal), new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("contains").Invoke(a[0], a[1], a[2])));
#else
            // Some overloads are not available in .NET Framework, therefore we substitute some with custom extensions
            AddMethodTranslator(() => NetFrameworkExtensions.Contains("", "", StringComparison.Ordinal), new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("contains").Invoke(a[1], a[2], a[3])));
#endif
        }

        private void AddFrameworkDependentSplitMehtodTranslations()
        {
            var splitCompiler = new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("split").Invoke(args));
            AddMethodTranslator(() => "".Split("", StringSplitOptions.None), translator: splitCompiler);
            AddMethodTranslator(() => "".Split('-', StringSplitOptions.None), translator: splitCompiler);
            AddMethodTranslator(() => "".Split(new char[0]), new GenericMethodCompiler(args => args[0].Member("split").Invoke(args[1])));

#if !DotNetCore
            AddMethodTranslator(() => "".Trim(new char[0]), new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimEnd").Invoke(
                    new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimStart").Invoke(a[0], a[1].Indexer(0)), a[1].Clone().Indexer(0))));
            AddMethodTranslator(() => NetFrameworkExtensions.TrimStart(""), new GenericMethodCompiler(
                a => a[1].Member("trimStart").Invoke()));
            AddMethodTranslator(() => "".TrimStart(new char[0]), new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimStart").Invoke(a[0], a[1].Indexer(0))));
            AddMethodTranslator(() => NetFrameworkExtensions.TrimEnd(""), new GenericMethodCompiler(
                a => a[1].Member("trimEnd").Invoke()));
            AddMethodTranslator(() => "".TrimEnd(new char[0]), new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimEnd").Invoke(a[0], a[1].Indexer(0))));
#else
            AddMethodTranslator(() => "".Trim('-'), new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimEnd").Invoke(
                    new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimStart").Invoke(a[0], a[1]), a[1].Clone())));
            AddMethodTranslator(() => "".TrimStart(), new GenericMethodCompiler(
                a => a[0].Member("trimStart").Invoke()));
            AddMethodTranslator(() => "".TrimStart('-'), new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimStart").Invoke(a[0], a[1])));
            AddMethodTranslator(() => "".TrimEnd(), new GenericMethodCompiler(
                a => a[0].Member("trimEnd").Invoke()));
            AddMethodTranslator(() => "".TrimEnd('-'), new GenericMethodCompiler(
                a => new JsIdentifierExpression("dotvvm").Member("translations").Member("string").Member("trimEnd").Invoke(a[0], a[1])));
#endif

            AddMethodTranslator(() => "".Trim(), new GenericMethodCompiler(
                a => a[0].Member("trim").Invoke()));
            AddMethodTranslator(() => "".PadLeft(10), new GenericMethodCompiler(
                a => a[0].Member("padStart").Invoke(a[1])));
            AddMethodTranslator(() => "".PadLeft(10, '-'), new GenericMethodCompiler(
                a => a[0].Member("padStart").Invoke(a[1], a[2])));
            AddMethodTranslator(() => "".PadRight(10), new GenericMethodCompiler(
                a => a[0].Member("padEnd").Invoke(a[1])));
            AddMethodTranslator(() => "".PadRight(10, '-'), new GenericMethodCompiler(
                a => a[0].Member("padEnd").Invoke(a[1], a[2])));
        }

        private void AddDefaultMathTranslations()
        {
            AddMethodTranslator(() => Math.Abs(1), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("abs").Invoke(args[1])));
            AddMethodTranslator(() => Math.Abs(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("abs").Invoke(args[1])));
            AddMethodTranslator(() => Math.Acos(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("acos").Invoke(args[1])));
            AddMethodTranslator(() => Math.Asin(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("asin").Invoke(args[1])));
            AddMethodTranslator(() => Math.Atan(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("atan").Invoke(args[1])));
            AddMethodTranslator(() => Math.Atan2(1.0d, 1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("atan2").Invoke(args[1], args[2])));

            AddMethodTranslator(() => Math.Ceiling(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("ceil").Invoke(args[1])));
            AddMethodTranslator(() => Math.Cos(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("cos").Invoke(args[1])));
            AddMethodTranslator(() => Math.Cosh(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("cosh").Invoke(args[1])));

            AddMethodTranslator(() => Math.Exp(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("exp").Invoke(args[1])));

            AddMethodTranslator(() => Math.Floor(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("floor").Invoke(args[1])));

            AddMethodTranslator(() => Math.Log(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("log").Invoke(args[1])));
            AddMethodTranslator(() => Math.Log10(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("log10").Invoke(args[1])));

            AddMethodTranslator(() => Math.Max(1, 1), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("max").Invoke(args[1], args[2])));
            AddMethodTranslator(() => Math.Max(1.0d, 1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("max").Invoke(args[1], args[2])));
            AddMethodTranslator(() => Math.Min(1, 1), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("min").Invoke(args[1], args[2])));
            AddMethodTranslator(() => Math.Min(1.0d, 1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("min").Invoke(args[1], args[2])));

            AddMethodTranslator(() => Math.Pow(1.0d, 1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("pow").Invoke(args[1], args[2])));

            AddMethodTranslator(() => Math.Round(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("round").Invoke(args[1])));
            AddMethodTranslator(() => Math.Round(1.0d, 1), new GenericMethodCompiler(
                args => args[1].Member("toFixed").Invoke(args[2])));

            AddMethodTranslator(() => Math.Sign(1), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("sign").Invoke(args[1])));
            AddMethodTranslator(() => Math.Sign(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("sign").Invoke(args[1])));
            AddMethodTranslator(() => Math.Sin(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("sin").Invoke(args[1])));
            AddMethodTranslator(() => Math.Sinh(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("sinh").Invoke(args[1])));
            AddMethodTranslator(() => Math.Sqrt(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("sqrt").Invoke(args[1])));

            AddMethodTranslator(() => Math.Tan(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("tan").Invoke(args[1])));
            AddMethodTranslator(() => Math.Tanh(1.0d), new GenericMethodCompiler(
                args => new JsIdentifierExpression("Math").Member("tanh").Invoke(args[1])));
            AddMethodTranslator(() => Math.Truncate(1.0d), new GenericMethodCompiler(
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

            AddMethodTranslator(() => Enumerable.All(Enumerable.Empty<Generic.T>(), _ => false), new GenericMethodCompiler(args => args[1].Member("every").Invoke(args[2])));
            AddMethodTranslator(() => Enumerable.Any(Enumerable.Empty<Generic.T>()), new GenericMethodCompiler(args => args[1].Member("some").Invoke(returnTrueFunc.Clone())));
            AddMethodTranslator(() => Enumerable.Any(Enumerable.Empty<Generic.T>(), _ => false), new GenericMethodCompiler(args => args[1].Member("some").Invoke(args[2])));
            AddMethodTranslator(() => Enumerable.Concat(Enumerable.Empty<Generic.T>(), Enumerable.Empty<Generic.T>()), new GenericMethodCompiler(args => args[1].Member("concat").Invoke(args[2])));
            AddMethodTranslator(() => Enumerable.Count(Enumerable.Empty<Generic.T>()), new GenericMethodCompiler(args => args[1].Member("length")));
            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().Distinct(), new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("distinct").Invoke(args[1]),
                check: (method, target, arguments) => EnsureIsComparableInJavascript(method, ReflectionUtils.GetEnumerableType(arguments.First().Type).NotNull())));

            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().ElementAt(0),
                new GenericMethodCompiler((args, method) => BuildIndexer(args[1], args[2], method)));

            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().FirstOrDefault(), new GenericMethodCompiler((args, m) =>
                args[1].Indexer(0)
                    .WithAnnotation(new VMPropertyInfoAnnotation(m.ReturnType)).WithAnnotation(MayBeNullAnnotation.Instance)));
            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().FirstOrDefault(_ => true), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("firstOrDefault").Invoke(args[1], args[2]).WithAnnotation(MayBeNullAnnotation.Instance)));

            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().LastOrDefault(), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("lastOrDefault").Invoke(args[1], returnTrueFunc.Clone()).WithAnnotation(MayBeNullAnnotation.Instance)));
            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().LastOrDefault(_ => false), new GenericMethodCompiler(args =>
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

            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().OrderBy(_ => Generic.Enum.Something), new GenericMethodCompiler((jArgs, dArgs) => new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("orderBy")
                    .Invoke(jArgs[1], jArgs[2], new JsLiteral((IsDelegateReturnTypeEnum(dArgs.Last().Type)) ? GetDelegateReturnTypeHash(dArgs.Last().Type) : null)),
                check: (method, _, arguments) => EnsureIsComparableInJavascript(method, arguments.Last().Type.GetGenericArguments().Last())));
            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().OrderByDescending(_ => Generic.Enum.Something), new GenericMethodCompiler((jArgs, dArgs) => new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("orderByDesc")
                    .Invoke(jArgs[1], jArgs[2], new JsLiteral((IsDelegateReturnTypeEnum(dArgs.Last().Type)) ? GetDelegateReturnTypeHash(dArgs.Last().Type) : null)),
                check: (method, _, arguments) => EnsureIsComparableInJavascript(method, arguments.Last().Type.GetGenericArguments().Last())));

            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().Select(_ => Generic.Enum.Something),
                translator: new GenericMethodCompiler(args => args[1].Member("map").Invoke(args[2])));
            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().Skip(0), new GenericMethodCompiler(args => args[1].Member("slice").Invoke(args[2])));

            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().Take(0), new GenericMethodCompiler(args =>
                args[1].Member("slice").Invoke(new JsLiteral(0), args[2])));
            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().ToArray(), new GenericMethodCompiler(args => args[1]));
            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().ToList(), new GenericMethodCompiler(args => args[1]));

            AddMethodTranslator(() => Enumerable.Empty<Generic.T>().Where(_ => true), new GenericMethodCompiler(args => args[1].Member("filter").Invoke(args[2])));
        }

        private void AddDefaultDictionaryTranslations()
        {
            AddMethodTranslator(() => default(Dictionary<Generic.T, Generic.T>)!.Clear(), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("dictionary").Member("clear").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))));
            var containsKey = new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("dictionary").Member("containsKey").Invoke(args[0], args[1]));
            AddMethodTranslator(() => default(IDictionary<Generic.T, Generic.T>)!.ContainsKey(null!), containsKey);
            AddMethodTranslator(() => default(IReadOnlyDictionary<Generic.T, Generic.T>)!.ContainsKey(null!), containsKey);
            AddMethodTranslator(() => default(IDictionary<Generic.T, Generic.T>)!.Remove(null!), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("dictionary").Member("remove").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])));
        }

        private void AddDefaultListTranslations()
        {
            AddMethodTranslator(() => new List<Generic.T?>().Add(null), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("add").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])));
            AddMethodTranslator(() => new List<Generic.T>().AddRange(Enumerable.Empty<Generic.T>()), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("addRange").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])));
            AddMethodTranslator(() => new List<Generic.T>().Clear(), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("clear").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))));
            AddMethodTranslator(() => new List<Generic.T?>().Insert(0, null), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("insert").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1], args[2])));
            AddMethodTranslator(() => new List<Generic.T>().InsertRange(0, Enumerable.Empty<Generic.T>()), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("insertRange").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1], args[2])));
            AddMethodTranslator(() => new List<Generic.T>().RemoveAt(0), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("removeAt").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])));
            AddMethodTranslator(() => new List<Generic.T>().RemoveAll(_ => true), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("removeAll").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1])));
            AddMethodTranslator(() => new List<Generic.T>().RemoveRange(0, 10), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("removeRange").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[1], args[2])));
            AddMethodTranslator(() => new List<Generic.T>().Reverse(), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("reverse").Invoke(args[0].WithAnnotation(ShouldBeObservableAnnotation.Instance))));
            AddMethodTranslator(() => new List<Generic.T>().AsReadOnly(), new GenericMethodCompiler(args => args[0]));
            AddMethodTranslator(
               () => new List<Generic.T?>().Contains(null),
               new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("contains").Invoke(args[0], args[1]).WithAnnotation(MayBeNullAnnotation.Instance)));

            // DotVVM list extensions:
            AddMethodTranslator(
                () => ListExtensions.AddOrUpdate(new List<Generic.T?>(), null, _ => false, x => x),
                new GenericMethodCompiler(args =>
                    new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("addOrUpdate").Invoke(args[1].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[2], args[3], args[4])));
            AddMethodTranslator(() => ListExtensions.RemoveFirst(new List<Generic.T>(), _ => true), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("removeFirst").Invoke(args[1].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[2])));
            AddMethodTranslator(() => ListExtensions.RemoveLast(new List<Generic.T>(), _ => true), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("array").Member("removeLast").Invoke(args[1].WithAnnotation(ShouldBeObservableAnnotation.Instance), args[2])));
        }

        private void AddDefaultDateTimeTranslations()
        {
            JsExpression IncrementExpression(JsExpression left, int value)
                => new JsBinaryExpression(left, BinaryOperatorType.Plus, new JsLiteral(value));

            AddPropertyTranslator(() => DateTime.Now.Year, new GenericMethodCompiler(args =>
                new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getFullYear").Invoke()));
            AddPropertyTranslator(() => DateTime.Now.Month, new GenericMethodCompiler(args =>
                IncrementExpression(new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getMonth").Invoke(), 1)));
            AddPropertyTranslator(() => DateTime.Now.Day, new GenericMethodCompiler(args =>
                new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getDate").Invoke()));
            AddPropertyTranslator(() => DateTime.Now.Hour, new GenericMethodCompiler(args =>
                new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getHours").Invoke()));
            AddPropertyTranslator(() => DateTime.Now.Minute, new GenericMethodCompiler(args =>
                new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getMinutes").Invoke()));
            AddPropertyTranslator(() => DateTime.Now.Second, new GenericMethodCompiler(args =>
                new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getSeconds").Invoke()));
            AddPropertyTranslator(() => DateTime.Now.Millisecond, new GenericMethodCompiler(args =>
                new JsInvocationExpression(new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate"), args[0]).Member("getMilliseconds").Invoke()));

            AddMethodTranslator(() => DateTime.UtcNow.ToBrowserLocalTime(), new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("translations").Member("dateTime").Member("toBrowserLocalTime").Invoke(args[1].WithAnnotation(ShouldBeObservableAnnotation.Instance)).WithAnnotation(ResultIsObservableAnnotation.Instance)));
            AddMethodTranslator(() => default(DateTime?).ToBrowserLocalTime(), new GenericMethodCompiler(args =>
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
