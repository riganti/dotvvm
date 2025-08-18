#nullable enable
#pragma warning disable CS0618 // disable the warning about obsoletes

using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DotvvmPropertyTests
    {
        public class MockComponent : DotvvmBindableObject
        {
            public object? Property
            {
                get { return (object?)GetValue(PropertyProperty); }
                set { SetValue(PropertyProperty, value); }
            }
            public static DotvvmProperty PropertyProperty
                = DotvvmProperty.Register<object, MockComponent>(t => t.Property);
        }

        DotvvmConfiguration config => DotvvmTestHelper.DefaultConfig;
        BindingCompilationService bindingService => config.ServiceProvider.GetRequiredService<BindingCompilationService>();

        [TestMethod]
        public void DotvvmProperty_PropertyRegisteredTwiceThrowException()
        {
            Assert.ThrowsException<DotvvmProperty.PropertyAlreadyExistsException>(() => {
                _ = MockComponent.PropertyProperty; // calls the static ctor
                DotvvmProperty.Register<bool, MockComponent>(t => t.Property);
            });
        }

        [TestMethod]
        public void HtmlGenericControl_DoesNotContainIdProperty()
        {
            // call the HtmlGenericControl..cctor
            var capability = HtmlGenericControl.HtmlCapabilityProperty;
            var prop = capability.PropertyMapping!.Value.First(x => x.dotvvmProperty.Name == "ID");
            var prop2 = DotvvmProperty.ResolveProperty(typeof(HtmlGenericControl), "ID");

            Assert.IsNotNull(prop2);
            Assert.AreEqual("ID", prop2.Name);
            Assert.AreEqual(typeof(DotvvmControl), prop2.DeclaringType);
            Assert.AreEqual(prop.dotvvmProperty, prop2);
        }

        public class TestObject : DotvvmBindableObject
        {
            [MarkupOptions(Required = true)]
            public string? Aliased
            {
                get => (string?)GetValue(AliasedProperty);
                set => SetValue(AliasedProperty, value);
            }
            public static DotvvmProperty AliasedProperty
                = DotvvmProperty.Register<int, TestObject>(c => c.Aliased);

            // NB: In actual use, IsError should be set to true.
            [Obsolete("Use 'Aliased' instead.", false)]
            [PropertyAlias(nameof(Aliased))]
            public string? Alias
            {
                get => (string?)GetValue(AliasedProperty);
                set => SetValue(AliasedProperty, value);
            }
            public static DotvvmProperty AliasProperty
                = DotvvmProperty.RegisterAlias<TestObject>(c => c.Alias);
        }

        [ContainsDotvvmProperties]
        public class AttachedOne
        {
            [AttachedProperty(typeof(string))]
            public static readonly DotvvmProperty OneProperty =
                DotvvmProperty.Register<string, AttachedOne>(() => OneProperty, string.Empty);
        }

        [ContainsDotvvmProperties]
        public class AttachedTwo
        {
            [AttachedProperty(typeof(string))]
            [PropertyAlias("One", typeof(AttachedOne))]
            [Obsolete("Use 'AttachedOne.One' instead.")]
            public static readonly DotvvmProperty TwoProperty =
                DotvvmProperty.RegisterAlias<AttachedTwo>(() => TwoProperty);
        }

        [TestInitialize]
        public void Init()
        {
            DotvvmPropertyAlias.Resolve((DotvvmPropertyAlias)TestObject.AliasProperty);
            _ = AttachedOne.OneProperty;
            DotvvmPropertyAlias.Resolve((DotvvmPropertyAlias)AttachedTwo.TwoProperty);
        }

        [TestMethod]
        public void DotvvmProperty_AliasRegistered()
        {
            var resolvedAlias = DotvvmProperty.ResolveProperty(typeof(TestObject), nameof(TestObject.Alias));
            Assert.IsTrue(resolvedAlias == TestObject.AliasProperty);
            Assert.IsTrue(((DotvvmPropertyAlias)TestObject.AliasProperty).Aliased == TestObject.AliasedProperty);
        }

        [TestMethod]
        public void DotvvmProperty_AliasUsageThrows()
        {
            var dummy = new HtmlGenericControl("div");
            Assert.ThrowsException<NotSupportedException>(() => TestObject.AliasProperty.GetValue(dummy));
            Assert.ThrowsException<NotSupportedException>(() => TestObject.AliasProperty.IsSet(dummy));
            Assert.ThrowsException<NotSupportedException>(() => TestObject.AliasProperty.SetValue(dummy, ""));
        }

        [TestMethod]
        public void DotvvmProperty_AttachedAlias()
        {
            Assert.IsTrue(((DotvvmPropertyAlias)AttachedTwo.TwoProperty).Aliased == AttachedOne.OneProperty);
        }

        [TestMethod]
        public void DotvvmProperty_ObsoleteAttribute()
        {
            Assert.IsTrue(TestObject.AliasProperty.ObsoleteAttribute is object);
            var obsolete = typeof(TestObject)
                .GetProperty(nameof(TestObject.Alias))
                ?.GetCustomAttribute<ObsoleteAttribute>();
            Assert.IsTrue(TestObject.AliasProperty.ObsoleteAttribute?.Message == obsolete?.Message);
        }

        [TestMethod]
        public void DotvvmProperty_CompileTimeAliasing()
        {
            var tree = DotvvmTestHelper.ParseResolvedTree(
@"@viewModel System.Object
<span AttachedTwo.Two=Test></span>");
            Assert.IsFalse(tree.Content.First().TryGetProperty(AttachedTwo.TwoProperty, out _));
            Assert.IsTrue(tree.Content.First().TryGetProperty(AttachedOne.OneProperty, out _));
        }

        [TestMethod]
        public void DotvvmProperty_ObsoleteWarning()
        {
            var tree = DotvvmTestHelper.ParseResolvedTree(
@"@viewModel System.Object
<span AttachedTwo.Two=Test></span>");

            // the property is aliased so we need to get the alias
            if (tree.Content.First().TryGetProperty(AttachedOne.OneProperty, out var setter))
            {
                Assert.IsTrue(setter.DothtmlNode!.NodeWarnings.Any());
            }
        }

        [TestMethod]
        public void DotvvmProperty_CompileTimeAliasingError()
        {
            var tree = DotvvmTestHelper.ParseResolvedTree(
@"@viewModel System.Object
<span AttachedOne.One=Test1 AttachedTwo.Two=Test2></span>");
            Assert.ThrowsException<DotvvmCompilationException>(() => DotvvmTestHelper.CheckForErrors(tree.DothtmlNode));
        }

        [TestMethod]
        public void DotvvmProperty_GetDataContextType_ItemTemplate_List()
        {
            DotvvmProperty_GetDataContextType_ItemTemplate_Helper<List<string>, string>();
        }
        [TestMethod]
        public void DotvvmProperty_GetDataContextType_ItemTemplate_Enumerable()
        {
            DotvvmProperty_GetDataContextType_ItemTemplate_Helper<System.Collections.ArrayList, object>();
        }
        [TestMethod]
        public void DotvvmProperty_GetDataContextType_ItemTemplate_Grid()
        {
            DotvvmProperty_GetDataContextType_ItemTemplate_Helper<IPageableGridViewDataSet, object>();
        }
        [TestMethod]
        public void DotvvmProperty_GetDataContextType_ItemTemplate_GenericGrid()
        {
            DotvvmProperty_GetDataContextType_ItemTemplate_Helper<IGridViewDataSet<int>, int>();
        }
        [TestMethod]
        public void DotvvmProperty_GetDataContextType_ItemTemplate_Array()
        {
            DotvvmProperty_GetDataContextType_ItemTemplate_Helper<Guid[], Guid>();
        }
        public void DotvvmProperty_GetDataContextType_ItemTemplate_Helper<T, TElement>()
        {
            var repeater = new Repeater();
            var parentContext = DataContextStack.Create(typeof(T));
            repeater.SetDataContextType(parentContext);
            repeater.DataSource = ValueBindingExpression.CreateThisBinding<T>(bindingService, parentContext);

            var dc = Repeater.ItemTemplateProperty.GetDataContextType(repeater)!;
            Assert.AreEqual(typeof(TElement), dc.DataContextType);
            Assert.AreEqual(parentContext, dc.Parent);

            // with data context missing
            repeater.properties.Remove(Internal.DataContextTypeProperty);
            Assert.AreEqual(null, Repeater.ItemTemplateProperty.GetDataContextType(repeater));

            var placeholder = new HtmlGenericControl("div");
            placeholder.SetDataContextType(parentContext);
            placeholder.Children.Add(repeater);

            dc = Repeater.ItemTemplateProperty.GetDataContextType(repeater)!;
            Assert.AreEqual(typeof(TElement), dc.DataContextType);
            Assert.AreEqual(parentContext, dc.Parent);

            // no change here
            dc = Repeater.SeparatorTemplateProperty.GetDataContextType(repeater);
            Assert.AreEqual(parentContext, dc);
        }

        public object?[] GetExampleValues(Type type)
        {
            if (type.IsNullable())
                return GetExampleValues(type.UnwrapNullableType()).Concat(new object? [] { null }).ToArray();
            if (type.IsEnum)
                return Enum.GetValues(type).Cast<object>().ToArray();
            if (type == typeof(bool))
                return new object[] { true, false };
            if (type == typeof(string))
                return new object[] { "a", "b" };
            if (type == typeof(string[]))
                return new object[] { new [] {"a", "b"}, new string[0], new string[] {"XXXXXX"} };
            if (type == typeof(object))
                return new object[] { "a", "b" };
            if (type.IsNumericType())
                return new object[] { 1, 2, 3 }.Select(n => Convert.ChangeType(n, type)).ToArray();
            if (type == typeof(UploadedFilesCollection))
                return new object?[] { new UploadedFilesCollection() };
            if (type == typeof(Command))
                return new object?[] { null, (Command)(() => Task.CompletedTask) };
            if (type == typeof(Action<string>))
                return new object?[] { null, (Action<string>)(_ => {}) };
            if (typeof(DotvvmBindableObject).IsAssignableFrom(type))
                return new object?[] { null, Activator.CreateInstance(type) };
            if (typeof(ITemplate).IsAssignableFrom(type))
                return new object?[] { null, new DelegateTemplate(_ => new HtmlGenericControl()), new CloneTemplate(new HtmlGenericControl()) };
            if (type.IsAssignableFrom(typeof(ValueBindingExpression<bool>)))
                return new object?[] { null, ValueBindingExpression.CreateBinding(bindingService, _ => true, new JsIdentifierExpression("a")) };
            if (type.IsAssignableFrom(typeof(List<int>)))
                return new object?[] { new List<int>(), new List<int> { 100 } };
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return new object?[] { Activator.CreateInstance(type) };

            return new object?[] { null };
            // throw new NotSupportedException(type.ToString());
        }

        IEnumerable<Type> allControls =
            from t in typeof(DotvvmControl).Assembly.GetTypes()
            where !t.IsAbstract
            where typeof(DotvvmBindableObject).IsAssignableFrom(t)
            where t.GetConstructor(Type.EmptyTypes) != null
            select t;

        // test that the getter and setter of the properties are behaving reasonably
        // it's kinda easy to copy paste a different property into get/set
        [TestMethod]
        public void DotvvmProperty_CorrectGetAndSet()
        {
            foreach (var control in allControls)
            {
                var instance = (DotvvmBindableObject)Activator.CreateInstance(control)!;

                var properties = DotvvmProperty.ResolveProperties(control);

                foreach (var p in properties)
                {
                    // there are some exception to the rules...
                    if (p == DotvvmControl.ClientIDProperty)
                        continue;
                    // capability properties behave slightly differently
                    if (p is DotvvmCapabilityProperty)
                        continue;

                    if (p.PropertyInfo == null)
                        continue;

                    Assert.IsNotNull(p.PropertyInfo.GetMethod, $"There is no getter for {p}");

                    if (p.PropertyInfo.PropertyType != p.PropertyType)
                        Console.WriteLine(p);

                    instance.properties.Remove(p.Id);
                    Assert.AreEqual(p.DefaultValue, instance.GetValue(p), $"GetValue default value {p}");
                    Assert.AreEqual(p.DefaultValue, instance.GetValueRaw(p.Id), $"GetValue(id) default value {p}");
                    Assert.AreEqual(p.DefaultValue, p.PropertyInfo.GetValue(instance), $"Getter default value {p}");

                    foreach (var example in GetExampleValues(p.PropertyType))
                    {
                        instance.SetValue(p, example);
                        Assert.AreEqual(example, instance.GetValue(p), $"GetValue is behaving weird {p}");
                        Assert.AreEqual(example, instance.GetValueRaw(p.Id), $"GetValue(id) is behaving weird {p}");
                        Assert.AreEqual(example, p.PropertyInfo.GetValue(instance), $"Getter is broken in {p}");

                        if (p.Id.CanUseFastAccessors)
                        {
                            Assert.AreEqual(example, instance.properties.GetOrThrow(p.Id), "$instance.properties.GetOrThrow {p}");
                        }
                    }

                    if (p.PropertyInfo.SetMethod == null)
                        continue;

                    foreach (var example in GetExampleValues(p.PropertyType))
                    {
                        p.PropertyInfo.SetValue(instance, example);
                        Assert.AreEqual(example, p.PropertyInfo.GetValue(instance), $"Setter is broken in {p}");
                        Assert.AreEqual(example, instance.GetValue(p), $"Setter is broken in {p} (in a weird way)");
                    }
                }
            }
        }

        [TestMethod]
        public void DotvvmProperty_SanityChecks()
        {
            var properties = allControls.SelectMany(DotvvmProperty.ResolveProperties).ToArray();

            foreach (var p in properties)
            {
                if (p is not DotvvmCapabilityProperty)
                {
                    // default value is of the specified type
                    if (p.DefaultValue != null)
                        Assert.IsInstanceOfType(p.DefaultValue, p.PropertyType, $"{p}: {p.DefaultValue} is not instance of {p.PropertyType}");
                    else
                        Assert.IsFalse(p.PropertyType.IsValueType && !p.PropertyType.IsNullable(), $"{p}: default value is null, but property type is not nullable {p.PropertyType}");
                }

                if (p.PropertyInfo != null)
                    Assert.AreEqual(p.PropertyInfo.DeclaringType, p.DeclaringType);
            }
        }

        [TestMethod]
        public void DotvvmProperty_CheckCorrectValueInDataBinding()
        {
            foreach (var control in allControls)
            {
                var properties = DotvvmProperty.ResolveProperties(control);

                foreach (var p in properties)
                {
                    if (p.IsBindingProperty)
                    {
                        Assert.IsFalse(p.MarkupOptions.AllowHardCodedValue);
                    }
                }
            }
        }

        [TestMethod]
        public void DotvvmProperty_ManyItemsSetter()
        {
            var properties = Enumerable.Range(0, 1000).Select(i => HtmlGenericControl.AttributesGroupDescriptor.GetDotvvmProperty("data-" + i.ToString())).ToArray();

            var setter = PropertyDictionaryImpl.CreateBulkSetter(properties, Enumerable.Range(0, 1000).Select(i => (object?)i).ToArray());

            var control1 = new HtmlGenericControl("div");
            setter(control1);
            var control2 = new HtmlGenericControl("div");
            setter(control2);

            Assert.AreEqual(1000, control1.Properties.Count);
        }

        [TestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public void DotvvmProperty_ControlClone(bool manyAttributes, bool nestedControl)
        {
            var control = new HtmlGenericControl("div");
            control.CssStyles.Set("color", "red");
            control.Attributes.Set("something", "value");

            if (manyAttributes)
                for (int i = 0; i < 60; i++)
                    control.Attributes.Set("data-" + i.ToString(), i.ToString());

            if (nestedControl)
            {
                control.Properties.Add(Styles.ReplaceWithProperty, new HtmlGenericControl("span") { InnerText = "23" });
            }

            var clone = (HtmlGenericControl)control.CloneControl();

            Assert.AreEqual(control.TagName, clone.TagName);
            Assert.AreEqual(control.CssStyles["color"], "red");

            // change original
            Assert.IsFalse(clone.CssStyles.ContainsKey("abc"));
            control.CssStyles.Set("color", "blue");
            control.CssStyles.Set("abc", "1");
            Assert.AreEqual("red", clone.CssStyles["color"]);
            Assert.IsFalse(clone.CssStyles.ContainsKey("abc"));

            if (nestedControl)
            {
                var nestedClone = (HtmlGenericControl)clone.Properties[Styles.ReplaceWithProperty]!;
                var nestedOriginal = (HtmlGenericControl)control.Properties[Styles.ReplaceWithProperty]!;
                Assert.AreEqual("23", nestedClone.InnerText);
                // change clone this time
                nestedClone.InnerText = "24";
                Assert.AreEqual("23", nestedOriginal.InnerText);
                Assert.AreEqual("24", nestedClone.InnerText);
            }

            if (manyAttributes)
            {
                for (int i = 0; i < 60; i++)
                {
                    Assert.AreEqual(i.ToString(), control.Attributes["data-" + i.ToString()]);
                    Assert.AreEqual(i.ToString(), clone.Attributes["data-" + i.ToString()]);
                }
            }
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void DotvvmProperty_VirtualDictionary_Append(int testClone)
        {
            var control = new HtmlGenericControl("div");

            foreach (var i in Enumerable.Range(0, 50))
            {
                control.Attributes.Set($"data-{i}", i);

                if (testClone > 0)
                {
                    var clone = (HtmlGenericControl)control.CloneControl();
                    if (testClone == 2)
                        (control, clone) = (clone, control);

                    clone.Attributes.Set("something-else", "abc");
                    Assert.AreEqual("abc", clone.Attributes["something-else"]);
                    clone.Attributes.Set("data-5", -1);
                    Assert.AreEqual(-1, clone.Attributes["data-5"]);
                }

                Assert.AreEqual(i + 1, control.properties.Count());
                Assert.AreEqual(i + 1, control.Attributes.Count);
                Assert.IsTrue(control.Attributes.ContainsKey("data-" + i.ToString()));
                Assert.IsFalse(control.Attributes.ContainsKey("something-else"));
                Assert.AreEqual(i, control.Attributes["data-" + i.ToString()]);

                XAssert.Equal(Enumerable.Range(0, i+1).Cast<object>(), control.Attributes.Values);
            }
        }

        [TestMethod, Ignore]
        [Conditional("NET6_0_OR_GREATER")]
        public void DotvvmProperty_ParallelAccess_DoesntCrashProcess()
        {
            var properties = new DotvvmProperty[] {
                DotvvmBindableObject.DataContextProperty,
                DotvvmControl.IncludeInPageProperty,
                HtmlGenericControl.VisibleProperty,
                TextBox.EnabledProperty,
                HtmlGenericControl.AttributesGroupDescriptor.GetDotvvmProperty("data-1"),
                HtmlGenericControl.AttributesGroupDescriptor.GetDotvvmProperty("data-2"),
                HtmlGenericControl.AttributesGroupDescriptor.GetDotvvmProperty("data-3"),
                HtmlGenericControl.AttributesGroupDescriptor.GetDotvvmProperty("data-4"),
                Button.EnabledProperty,
                FormControls.EnabledProperty
            };
            var control = new PlaceHolder();

            var exceptions = new ConcurrentBag<Exception>();

            ThreadPool.SetMinThreads(100, 100);
            Parallel.For(0, 10_000_000_000, new ParallelOptions { MaxDegreeOfParallelism = 100 }, i => {
                try
                {
                    var value = control.GetValue(properties[(i / 2) % properties.Length]);
                    control.properties.TryGet(properties[i % properties.Length], out value);
                    if (i % 2 == 0)
                    {
                        control.SetValue(properties[i % properties.Length], BoxingUtils.Box(i % 2 == 1));
                    }
                    else
                    {
                        control.properties.TryAdd(properties[i % properties.Length], BoxingUtils.Box(i % 2 == 1));
                    }

                    if (i % 16 == 0)
                    {
                        switch (Random.Shared.Next(0, 4))
                        {
                            case 0:
                                control = new PlaceHolder();
                                break;
                            case 1:
                                control = (PlaceHolder)control.CloneControl();
                                break;
                            case 2:
                                foreach (var prop in control.Properties.Keys)
                                    control.Properties.Remove(prop);
                                break;
                            case 3:
                                foreach (var prop in control.properties.PropertyGroup(HtmlGenericControl.AttributesGroupDescriptor.Id))
                                    control.properties.Remove(prop.Key);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    control = new PlaceHolder();
                }
                // if (control.properties.Count() >= 16)
                //     throw new Exception("Too many properties");
            });

            var exceptionGroups = exceptions
                .GroupBy(e => e.GetType().Name + ": " + e.Message)
                // .GroupBy(e => e.ToString())
                .Select(g => (g.Key, g.Count()))
                .OrderByDescending(g => g.Item2)
                .ToList();
            foreach (var (key, count) in exceptionGroups)
            {
                Console.WriteLine($"{key}: {count}");
            }
            if (exceptions.Count > 0)
            {
                Assert.Fail($"There were {exceptions.Count} exceptions thrown during the test. See the output for details.");
            }
        }

        [DataTestMethod]
        [DataRow(0, 0, 0)]
        [DataRow(1, 1, 1)]
        [DataRow(30, 1, 1)]
        [DataRow(1, 30, 1)]
        [DataRow(1, 1, 30)]
        [DataRow(1, 1, 8)]
        [DataRow(1, 8, 1)]
        [DataRow(8, 1, 1)]
        [DataRow(5, 5, 8)]
        [DataRow(5, 8, 5)]
        [DataRow(8, 5, 5)]
        [DataRow(5, 5, 5)]
        [DataRow(8, 8, 8)]
        public void PropertyGroup_Clear(int attributeCount, int classCount, int styleCount)
        {
            var control = new HtmlGenericControl();
            control.InnerText = "test-inner-text";
            var attributes = control.Attributes;

            (int, int, int) counts() => (control.Attributes.Count, control.CssClasses.Count, control.CssStyles.Count);
            Assert.AreEqual(0, control.Attributes.Count);
            Assert.AreEqual(0, control.CssClasses.Count);
            Assert.AreEqual(0, control.CssStyles.Count);

            for (int i = 0; i < attributeCount; i++)
                attributes.Set($"test-attribute-{i}", $"value{i}");

            Assert.AreEqual((attributeCount, 0, 0), counts());

            for (int i = 0; i < classCount; i++)
                control.CssClasses.Add($"test-class-{i}", true);

            Assert.AreEqual((attributeCount, classCount, 0), counts());


            for (int i = 0; i < styleCount; i++)
                control.CssStyles.Add($"test-style-{i}", $"value{i}");

            Assert.AreEqual((attributeCount, classCount, styleCount), counts());

            control.CssClasses.Clear();
            var checkpoint1 = control.CloneControl();
            Assert.AreEqual((attributeCount, 0, styleCount), counts());
            control.CssClasses.Add("another-class", true);
            Assert.AreEqual((attributeCount, 1, styleCount), counts());

            for (int i = 0; i < attributeCount; i++)
                Assert.AreEqual(attributes[$"test-attribute-{i}"], $"value{i}");

            for (int i = 0; i < classCount; i++)
                Assert.IsFalse(control.CssClasses.ContainsKey($"test-class-{i}"));

            for (int i = 0; i < styleCount; i++)
                Assert.AreEqual($"value{i}", control.CssStyles[$"test-style-{i}"]);

            control.Attributes.Clear();

            Assert.AreEqual((0, 1, styleCount), counts());
            var checkpoint2 = control.CloneControl();

            for (int i = 0; i < attributeCount; i++)
                Assert.IsFalse(attributes.ContainsKey($"test-attribute-{i}"));
            for (int i = 0; i < classCount; i++)
                Assert.IsFalse(control.CssClasses.ContainsKey($"test-class-{i}"));
            for (int i = 0; i < styleCount; i++)
                Assert.AreEqual($"value{i}", control.CssStyles[$"test-style-{i}"]);

            Assert.IsTrue(control.CssClasses["another-class"]);

            control.CssStyles.Clear();

            for (int i = 0; i < styleCount; i++)
                Assert.IsFalse(control.CssStyles.ContainsKey($"test-style-{i}"), $"Style 'test-style-{i}' should not exist.");

            Assert.IsTrue(control.CssClasses["another-class"]);

            Assert.AreEqual("test-inner-text", control.InnerText);

            control = (HtmlGenericControl)checkpoint1;
            Assert.AreEqual((attributeCount, 0, styleCount), counts());
            control = (HtmlGenericControl)checkpoint2;
            Assert.AreEqual((0, 1, styleCount), counts());
        }

        [TestMethod]
        public void PropertyIds_MatchGeneratedConstants()
        {
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyIds.DotvvmControl_ClientID, (uint)DotvvmControl.ClientIDProperty.Id);
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyIds.DotvvmControl_IncludeInPage, (uint)DotvvmControl.IncludeInPageProperty.Id);
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyIds.HtmlGenericControl_Visible, (uint)HtmlGenericControl.VisibleProperty.Id);
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyIds.HtmlGenericControl_InnerText, (uint)HtmlGenericControl.InnerTextProperty.Id);
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyIds.Literal_Text, (uint)Literal.TextProperty.Id);
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyIds.ButtonBase_Click, (uint)Button.ClickProperty.Id);
        }

        [TestMethod]
        public void PropertyGroupIds_MatchGeneratedConstants()
        {
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyGroupIds.HtmlGenericControl_Attributes, HtmlGenericControl.AttributesGroupDescriptor.Id);
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyGroupIds.HtmlGenericControl_CssClasses, HtmlGenericControl.CssClassesGroupDescriptor.Id);
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyGroupIds.HtmlGenericControl_CssStyles, HtmlGenericControl.CssStylesGroupDescriptor.Id);
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyGroupIds.RouteLink_Params, RouteLink.ParamsGroupDescriptor.Id);
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyGroupIds.RouteLink_QueryParameters, RouteLink.QueryParametersGroupDescriptor.Id);
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyGroupIds.JsComponent_Props, JsComponent.PropsGroupDescriptor.Id);
            Assert.AreEqual(DotvvmPropertyIdAssignment.PropertyGroupIds.JsComponent_Templates, JsComponent.TemplatesGroupDescriptor.Id);
        }

        [TestMethod]
        public void PropertyIds_AreUnique()
        {
            XAssert.Distinct(DotvvmProperty.AllProperties.Select(p => p.Id));
        }

        [TestMethod]
        public void PropertyGroupIds_AreUnique()
        {
            XAssert.Distinct(DotvvmPropertyGroup.AllGroups.Select(g => g.Id));
        }
    }
}
