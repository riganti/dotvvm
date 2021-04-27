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
            AddMethodTranslator(typeof(Enums), "GetNames", new EnumGetNamesMethodTranslator(), 0);

            JsExpression indexer(JsExpression[] args, MethodInfo method) =>
                BuildIndexer(args[0], args[1], method.DeclaringType.GetProperty("Item"));
            AddMethodTranslator(typeof(IList), "get_Item", new GenericMethodCompiler(indexer));
            AddMethodTranslator(typeof(IList<>), "get_Item", new GenericMethodCompiler(indexer));
            AddMethodTranslator(typeof(List<>), "get_Item", new GenericMethodCompiler(indexer));
            AddPropertyGetterTranslator(typeof(Nullable<>), "Value", new GenericMethodCompiler((args, method) => args[0]));
            AddPropertyGetterTranslator(typeof(Nullable<>), "HasValue",
                new GenericMethodCompiler(args => new JsBinaryExpression(args[0], BinaryOperatorType.NotEqual, new JsLiteral(null))));
            //AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Count), lengthMethod, new[] { typeof(IEnumerable) });

            JsBindingApi.RegisterJavascriptTranslations(this);
            BindingApi.RegisterJavascriptTranslations(this);
            BindingPageInfo.RegisterJavascriptTranslations(this);
            BindingCollectionInfo.RegisterJavascriptTranslations(this);

            AddPropertyGetterTranslator(typeof(Task<>), "Result", new GenericMethodCompiler(args => FunctionalExtensions.ApplyAction(args[0], a => a.RemoveAnnotations(typeof(ViewModelInfoAnnotation)))));

            AddMethodTranslator(typeof(DotvvmBindableObject).GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(m => m.Name == "GetValue" && !m.ContainsGenericParameters), new GenericMethodCompiler(
                args => {
                    var dotvvmproperty = ((DotvvmProperty)((JsLiteral)args[1]).Value);
                    return JavascriptTranslationVisitor.TranslateViewModelProperty(args[0], (MemberInfo)dotvvmproperty.PropertyInfo ?? dotvvmproperty.PropertyType.GetTypeInfo(), name: dotvvmproperty.Name);
                }
            ));

            AddDefaultToStringTranslations();
            AddDefaultStringTranslations();
            AddDefaultEnumerableTranslations();
            AddDefaultListTranslations();
            AddDefaultMathTranslations();
        }

        private void AddDefaultToStringTranslations()
        {
            AddMethodTranslator(typeof(object), "ToString", new GenericMethodCompiler(
                a => new JsIdentifierExpression("String").Invoke(a[0]), (m, c, a) => ToStringCheck(c)), 0);
            AddMethodTranslator(typeof(Convert), "ToString", new GenericMethodCompiler(
                a => new JsIdentifierExpression("String").Invoke(a[1]), (m, c, a) => ToStringCheck(a[0])), 1, true);

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
        }

        private void AddDefaultStringTranslations()
        {
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

            AddMethodTranslator(typeof(string), nameof(string.IndexOf), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => a[0].Member("indexOf").Invoke(a[1])));
            AddMethodTranslator(typeof(string), nameof(string.IndexOf), parameters: new[] { typeof(string), typeof(int) }, translator: new GenericMethodCompiler(
                a => a[0].Member("indexOf").Invoke(a[1], a[2])));
            AddMethodTranslator(typeof(string), nameof(string.LastIndexOf), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => a[0].Member("lastIndexOf").Invoke(a[1])));
            AddMethodTranslator(typeof(string), nameof(string.LastIndexOf), parameters: new[] { typeof(string), typeof(int) }, translator: new GenericMethodCompiler(
                a => a[0].Member("lastIndexOf").Invoke(a[1], a[2])));
            AddMethodTranslator(typeof(string), nameof(string.ToUpper), parameterCount: 0, translator: new GenericMethodCompiler(
                a => a[0].Member("toUpperCase").Invoke()));
            AddMethodTranslator(typeof(string), nameof(string.ToLower), parameterCount: 0, translator: new GenericMethodCompiler(
                a => a[0].Member("toLowerCase").Invoke()));
            AddMethodTranslator(typeof(string), nameof(string.Contains), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => a[0].Member("includes").Invoke(a[1])));
            AddMethodTranslator(typeof(string), nameof(string.StartsWith), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => a[0].Member("startsWith").Invoke(a[1])));
            AddMethodTranslator(typeof(string), nameof(string.EndsWith), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => a[0].Member("endsWith").Invoke(a[1])));
            AddMethodTranslator(typeof(string), nameof(string.IsNullOrEmpty), parameters: new[] { typeof(string) }, translator: new GenericMethodCompiler(
                a => new JsBinaryExpression(
                    new JsBinaryExpression(a[1], BinaryOperatorType.Equal, new JsLiteral(null)),
                    BinaryOperatorType.ConditionalOr,
                    new JsBinaryExpression(a[1].Clone(), BinaryOperatorType.StrictlyEqual, new JsLiteral("")))));

            var joinStringArrayMethod = typeof(string).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(string.Join) && m.GetParameters().Length == 2 && m.GetParameters().Last().ParameterType == typeof(string[]) && m.GetParameters().First().ParameterType == typeof(string)).Single();
            AddMethodTranslator(joinStringArrayMethod, translator: new GenericMethodCompiler(args => args[2].Member("join").Invoke(args[1])));
            var joinStringEnumerableMethod = typeof(string).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(string.Join) && m.GetParameters().Length == 2 && m.GetParameters().Last().ParameterType == typeof(IEnumerable<string>) && m.GetParameters().First().ParameterType == typeof(string)).Single();
            AddMethodTranslator(joinStringEnumerableMethod, translator: new GenericMethodCompiler(args => args[2].Member("join").Invoke(args[1])));

            AddMethodTranslator(typeof(string), nameof(string.Replace), parameters: new[] { typeof(string), typeof(string) }, translator: new GenericMethodCompiler(
                args => args[0].Member("split").Invoke(args[1]).Member("join").Invoke(args[2])));

            var splitMethod = typeof(string).GetMethods(BindingFlags.Public | BindingFlags.Instance).SingleOrDefault(m => m.Name == nameof(string.Split)
                && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(string) && m.GetParameters()[1].ParameterType == typeof(StringSplitOptions));
            var genericSplitCompiler = new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("stringHelper").Member("split").Invoke(args[0], args[1], args[2]));
            var genericExtensionSplitCompiler = new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("stringHelper").Member("split").Invoke(args[1], args[2], args[3]));
            if (splitMethod == null)
            {
                // Some overloads are not available in .NET Framework, therefore we substitute some with custom extensions
                AddMethodTranslator(typeof(NetFrameworkExtensions), nameof(NetFrameworkExtensions.Split), parameters: new[] { typeof(string), typeof(char), typeof(StringSplitOptions) },
                    translator: genericExtensionSplitCompiler);
                AddMethodTranslator(typeof(NetFrameworkExtensions), nameof(NetFrameworkExtensions.Split), parameters: new[] { typeof(string), typeof(string), typeof(StringSplitOptions) },
                    translator: genericExtensionSplitCompiler);
            }
            else
            {
                AddMethodTranslator(typeof(string), nameof(string.Split), parameters: new[] { typeof(char), typeof(StringSplitOptions) }, translator: genericSplitCompiler);
                AddMethodTranslator(typeof(string), nameof(string.Split), parameters: new[] { typeof(string), typeof(StringSplitOptions) }, translator: genericSplitCompiler);
            }

            AddMethodTranslator(typeof(string), nameof(NetFrameworkExtensions.Split), parameters: new[] { typeof(char[]) },
                    translator: new GenericMethodCompiler(args => args[0].Member("split").Invoke(args[1])));
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

        private void AddDefaultEnumerableTranslations()
        {
            var returnTrueFunc = new JsFunctionExpression(new[] { new JsIdentifier("arg") }, new JsBlockStatement(new JsReturnStatement(new JsLiteral(true))));
            var selectIdentityFunc = new JsFunctionExpression(new[] { new JsIdentifier("arg") },
                new JsBlockStatement(new JsReturnStatement(new JsIdentifierExpression("ko").Member("unwrap").Invoke(new JsIdentifierExpression("arg")))));

            bool EnsureIsComparableInJavascript(MethodInfo method, Type type)
            {
                if (!ReflectionUtils.IsPrimitiveType(type))
                    throw new DotvvmCompilationException($"Can not translate invocation of method \"{method.Name}\" to JavaScript. Comparison of non-primitive types is not supported.");

                return true;
            }

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.All), parameterCount: 2, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("all").Invoke(args[1], args[2])));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Any), parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("any").Invoke(args[1], returnTrueFunc.Clone())));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Any), parameterCount: 2, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("any").Invoke(args[1], args[2])));

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Concat), parameterCount: 2, translator: new GenericMethodCompiler(args =>
                args[1].Member("concat").Invoke(args[2])));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Count), parameterCount: 1, translator: new GenericMethodCompiler(a => a[1].Member("length")));

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Distinct), parameterCount: 1,
                translator: new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("distinct").Invoke(args[1]),
                check: (method, target, arguments) => EnsureIsComparableInJavascript(method, target?.Type ?? ReflectionUtils.GetEnumerableType(arguments.First().Type))));

            AddMethodTranslator(typeof(Enumerable).GetMethod("ElementAt", BindingFlags.Static | BindingFlags.Public), new GenericMethodCompiler((args, method) =>
                BuildIndexer(args[1], args[2], method)));

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.FirstOrDefault), parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("firstOrDefault").Invoke(args[1], returnTrueFunc.Clone()).WithAnnotation(ResultIsObservableAnnotation.Instance)));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.FirstOrDefault), parameterCount: 2, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("firstOrDefault").Invoke(args[1], args[2]).WithAnnotation(ResultIsObservableAnnotation.Instance)));

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.LastOrDefault), parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("lastOrDefault").Invoke(args[1], returnTrueFunc.Clone()).WithAnnotation(ResultIsObservableAnnotation.Instance)));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.LastOrDefault), parameterCount: 2, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("lastOrDefault").Invoke(args[1], args[2]).WithAnnotation(ResultIsObservableAnnotation.Instance)));

            foreach (var type in new[] { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) })
            {
                AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Max), parameters: new[] { typeof(IEnumerable<>).MakeGenericType(type) }, translator: new GenericMethodCompiler(args =>
                    new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("max").Invoke(args[1], selectIdentityFunc.Clone())));
                var maxSelect = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == nameof(Enumerable.Max) && m.GetParameters().Length == 2 && m.GetParameters().Last().ParameterType.GetGenericArguments().Last() == type).Single();
                AddMethodTranslator(maxSelect, translator: new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("max").Invoke(args[1], args[2])));

                AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Min), parameters: new[] { typeof(IEnumerable<>).MakeGenericType(type) }, translator: new GenericMethodCompiler(args =>
                    new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("min").Invoke(args[1], selectIdentityFunc.Clone())));
                var minSelect = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == nameof(Enumerable.Min) && m.GetParameters().Length == 2 && m.GetParameters().Last().ParameterType.GetGenericArguments().Last() == type).Single();
                AddMethodTranslator(minSelect, translator: new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("min").Invoke(args[1], args[2])));
            }

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.OrderBy), parameterCount: 2,
                translator: new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("orderBy").Invoke(args[1], args[2]),
                check: (method, _, arguments) => EnsureIsComparableInJavascript(method, arguments.Last().Type.GetGenericArguments().Last())));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.OrderByDescending), parameterCount: 2,
                translator: new GenericMethodCompiler(args => new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("orderByDesc").Invoke(args[1], args[2]),
                check: (method, _, arguments) => EnsureIsComparableInJavascript(method, arguments.Last().Type.GetGenericArguments().Last())));

            var selectMethod = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(Enumerable.Select) && m.GetParameters().Length == 2 && m.GetParameters().Last().ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)).Single();
            AddMethodTranslator(selectMethod, translator: new GenericMethodCompiler(args => args[1].Member("map").Invoke(args[2])));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Skip), parameterCount: 2, translator: new GenericMethodCompiler(args =>
                args[1].Member("slice").Invoke(args[2])));

            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Take), parameterCount: 2, translator: new GenericMethodCompiler(args =>
                args[1].Member("slice").Invoke(new JsLiteral(0), args[2])));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.ToArray), parameterCount: 1, translator: new GenericMethodCompiler(args => args[1]));
            AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.ToList), parameterCount: 1, translator: new GenericMethodCompiler(args => args[1]));

            var whereMethod = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(Enumerable.Where) && m.GetParameters().Length == 2 && m.GetParameters().Last().ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)).Single();
            AddMethodTranslator(whereMethod, translator: new GenericMethodCompiler(args => args[1].Member("filter").Invoke(args[2])));
        }

        private void AddDefaultListTranslations()
        {
            AddMethodTranslator(typeof(List<>), "Add", parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("add").Invoke(args[0].Clone(), args[1], args[0].EnsureObservableWrapped())));
            AddMethodTranslator(typeof(List<>), "AddRange", parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("addRange").Invoke(args[0].Clone(), args[1], args[0].EnsureObservableWrapped())));
            AddMethodTranslator(typeof(List<>), "Clear", parameterCount: 0, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("clear").Invoke(args[0].EnsureObservableWrapped())));
            AddMethodTranslator(typeof(List<>), "ForEach", parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("forEach").Invoke(args[0].Clone(), args[1], args[0].EnsureObservableWrapped())));
            AddMethodTranslator(typeof(List<>), "Insert", parameterCount: 2, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("insert").Invoke(args[0].Clone(), args[1], args[2], args[0].EnsureObservableWrapped())));
            AddMethodTranslator(typeof(List<>), "InsertRange", parameterCount: 2, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("insertRange").Invoke(args[0].Clone(), args[1], args[2], args[0].EnsureObservableWrapped())));
            AddMethodTranslator(typeof(List<>), "RemoveAt", parameterCount: 1, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("removeAt").Invoke(args[0].Clone(), args[1], args[0].EnsureObservableWrapped())));
            AddMethodTranslator(typeof(List<>), "RemoveRange", parameterCount: 2, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("removeRange").Invoke(args[0].Clone(), args[1], args[2], args[0].EnsureObservableWrapped())));
            AddMethodTranslator(typeof(List<>), "Reverse", parameterCount: 0, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("reverse").Invoke(args[0].Clone(), args[0].EnsureObservableWrapped())));

            // DotVVM list extensions:
            AddMethodTranslator(typeof(ListExtensions), "AddOrUpdate", parameterCount: 4, translator: new GenericMethodCompiler(args =>
                new JsIdentifierExpression("dotvvm").Member("arrayHelper").Member("addOrUpdate").Invoke(args[1].Clone(), args[2], args[3], args[4], args[1].EnsureObservableWrapped())));
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
                var method2 = genericType.GetMethod(method.Name,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                    binder: null, types: method.GetParameters().Select(p => p.ParameterType).ToArray(), modifiers: null);

                if (method2 == null)
                {
                    method2 = genericType.GetMethod(method.Name,
                        BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                }

                if (method2 != null)
                {
                    var r2 = TryTranslateCall(context, args, method2);
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
