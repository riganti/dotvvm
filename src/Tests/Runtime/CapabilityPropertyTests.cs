using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class CapabilityPropertyTests
    {
        // initialize properties
        IControlResolver x = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<IControlResolver>();

        [DataTestMethod]
        [DataRow(typeof(TestControl1), "Something", "blbost")]
        [DataRow(typeof(TestControl1), "SomethingElse", "baf")]
        [DataRow(typeof(TestControl1), "Visible", true)]
        [DataRow(typeof(TestControl1), "ID", null)]
        [DataRow(typeof(TestControl2), "ID", null)]
        [DataRow(typeof(TestControl2), "Something", "blbost2")]
        [DataRow(typeof(TestControl2), "SomethingElse", "baf")]
        [DataRow(typeof(TestControl2), "AnotherHtml:Visible", true)]
        [DataRow(typeof(TestControl3), "Something", "abc")]
        [DataRow(typeof(TestControl3), "SomethingElse", "baf")]
        [DataRow(typeof(TestControl4), "Something", "abc")]
        [DataRow(typeof(TestControl4), "SomethingElse", "baf")]
        [DataRow(typeof(TestControl5), "SomethingElse", "baf")]
        [DataRow(typeof(TestControl5), "AnotherTest:SomethingElse", "baf")]
        [DataRow(typeof(TestControl5), "ItemSomethingElse", "baf")]
        public void ControlDefaultValues(Type control, string property, object value)
        {
            var c = (DotvvmBindableObject)Activator.CreateInstance(control);
            var allProps = DotvvmProperty.ResolveProperties(control).Select(p => p.Name);
            var prop = DotvvmProperty.ResolveProperty(control, property);
            Assert.IsNotNull(prop, $"{control.Name}.{property} is not defined. These properties exist: {string.Join(", ", allProps)}");
            var realValue = c.GetValue(prop);

            Assert.AreEqual(value, realValue);
        }

        [DataTestMethod]
        [DataRow(typeof(TestControl1), "HtmlCapability;TestCapability")]
        [DataRow(typeof(TestControl2), "HtmlCapability;HtmlCapability;TestCapability")]
        [DataRow(typeof(TestControl3), "HtmlCapability;TestCapability;TestNestedCapability")]
        [DataRow(typeof(TestControl4), "HtmlCapability;TestCapability;TestNestedCapability")]
        [DataRow(typeof(TestControl5), "HtmlCapability;TestCapability;TestCapability;TestCapability;TestNestedCapabilityWithPrefix")]
        [DataRow(typeof(HtmlGenericControl), "HtmlCapability")]
        [DataRow(typeof(Button), "HtmlCapability;TextOrContentCapability")]
        public void ControlRegisteredCapabilities(Type control, string capabilities)
        {
            var c = DotvvmCapabilityProperty.GetCapabilities(control).Select(c => c.PropertyType.Name).ToArray();
            Array.Sort(c);

            Assert.AreEqual(
                capabilities,
                string.Join(";", c)
            );
        }


        [TestMethod]
        public void TestCapability_GetterAndSetter1()
        {
            var control = new TestControl1 { Something = "X" };
            Assert.AreEqual(
                new TestCapability { Something = "X" },
                control.GetCapability<TestCapability>()
            );
            control.SetCapability(new TestCapability { Something = "Y", SomethingElse = "Z" });
            Assert.AreEqual(
                new TestCapability { Something = "Y", SomethingElse = "Z" },
                control.GetCapability<TestCapability>()
            );
            Assert.AreEqual("Y", control.Something);
            Assert.AreEqual("Z", control.GetValue<string>("SomethingElse"));
            Assert.AreEqual("Z", control.GetValue(c => c.GetCapability<TestCapability>().SomethingElse));

            control.SetCapability(new HtmlCapability { Attributes = { ["attr"] = new("a") } });
            Assert.AreEqual("a", control.Attributes["attr"]);
            control.AddAttribute("class", "x");
            control.AddAttribute("class", "y");
            var attrs = control.GetValue(c => c.GetCapability<HtmlCapability>()).Attributes;
            Assert.AreEqual("x y", attrs["class"].GetValue());
            Assert.AreEqual("a", attrs["attr"].GetValue());
            Assert.AreEqual("x y", control.GetValue(c => c.GetCapability<HtmlCapability>().Attributes["class"].ValueOrDefault));
        }
        [TestMethod]
        public void BitMoreComplexCapability_DefaultValues()
        {
            var c = new TestControlFallbackProps().GetCapability<BitMoreComplexCapability>();

            Assert.AreEqual(null, c.BindingOnly);
            Assert.AreEqual(30, c.NotNullable);
            Assert.AreEqual(null, c.Nullable);
            Assert.AreEqual(null, c.ValueOrBinding.GetValue());
            Assert.AreEqual(null, c.ValueOrBindingNullable);
        }

        [TestMethod]
        public void BitMoreComplexCapability_NullableValueOrBinding()
        {
            var control = new TestControlFallbackProps();
            Assert.AreEqual(10, control.GetValue<int>("ValueOrBindingNullable2"));
            
            var c = control.GetCapability<BitMoreComplexCapability>();
            Assert.IsNull(c.ValueOrBindingNullable);

            control.ValueOrBindingNullable2 = 11;
            Assert.AreEqual(11, control.GetCapability<BitMoreComplexCapability>().ValueOrBindingNullable?.GetValue());

            control.ValueOrBindingNullable2 = 10;
            Assert.AreEqual(10, control.GetCapability<BitMoreComplexCapability>().ValueOrBindingNullable?.GetValue());

            control.Properties.Remove(TestControlFallbackProps.ValueOrBindingNullable2Property);
            Assert.IsNull(control.GetCapability<BitMoreComplexCapability>().ValueOrBindingNullable);
        }

        [TestMethod]
        public void BitMoreComplexCapability_SetNullableValueOrBinding()
        {
            var control = new TestControlFallbackProps() { ValueOrBindingNullable2 = 1 };
            Assert.AreEqual(1, control.ValueOrBindingNullable2);

            control.SetCapability(new BitMoreComplexCapability { ValueOrBindingNullable = new(2) });
            Assert.AreEqual(2, control.ValueOrBindingNullable2);

            control.SetCapability(new BitMoreComplexCapability { ValueOrBindingNullable = null });
            Assert.AreEqual(10, control.ValueOrBindingNullable2); // reverts to default
        }

        [TestMethod]
        public void BitMoreComplexCapability_GetterAndSetter()
        {
            var c = new TestControl6();
            Assert.AreEqual(null, c.GetValue<int?>("Nullable"));
            Assert.AreEqual(30, c.GetValue<int?>("NotNullable"));
            var defaultValues = c.GetCapability<BitMoreComplexCapability>();
            Assert.AreEqual(null, defaultValues.BindingOnly);
            Assert.AreEqual(null, defaultValues.ValueOrBinding.GetValue());
            Assert.AreEqual(null, defaultValues.ValueOrBindingNullable);
            Assert.AreEqual(null, defaultValues.Nullable);
            Assert.AreEqual(30, defaultValues.NotNullable);

            c.SetCapability(new BitMoreComplexCapability {
                NotNullable = 1,
                Nullable = 1,
                ValueOrBinding = new (1),
                ValueOrBindingNullable = new (1),
            });

            Assert.AreEqual(1, c.GetValue<int?>("Nullable"));
            Assert.AreEqual(1, c.GetValue<int?>("NotNullable"));
            Assert.AreEqual(1, c.GetValue<int?>("ValueOrBinding"));
            Assert.AreEqual(1, c.GetValue<int?>("ValueOrBindingNullable"));

            c.SetCapability(new BitMoreComplexCapability {
                NotNullable = 2,
                Nullable = null,
                ValueOrBinding = new((int?)null),
                ValueOrBindingNullable = null,
            });

            var values = c.GetCapability<BitMoreComplexCapability>();
            Assert.AreEqual(null, values.ValueOrBinding.GetValue());
            Assert.IsFalse(c.properties.Contains(c.GetDotvvmProperty("ValueOrBindingNullable")));
            Assert.AreEqual(null, values.ValueOrBindingNullable?.ValueOrDefault);
            Assert.AreEqual(2, values.NotNullable);
            Assert.AreEqual(null, values.Nullable);
        }
 
        [TestMethod]
        public void BitMoreComplexCapability_WeirdProperties_GetterAndSetter()
        {
            var config = DotvvmTestHelper.DefaultConfig;
            var bindingService = config.ServiceProvider.GetRequiredService<BindingCompilationService>();

            var controlF1 = new TestControlFallbackProps();
            var controlF2 = new TestControlFallbackProps();
            controlF1.Children.Add(controlF2);

            controlF1.BindingOnly = bindingService.Cache.CreateValueBinding<string>("'aaa'", DataContextStack.Create(typeof(string)));

            controlF2.ValueOrBinding2 = 1;
            controlF2.ValueOrBindingNullable2 = 11;

            var c = controlF2.GetCapability<BitMoreComplexCapability>();
            Assert.AreEqual(controlF1.BindingOnly, c.BindingOnly);
            Assert.AreEqual(30, c.NotNullable);
            Assert.AreEqual(null, c.Nullable);
            Assert.AreEqual(1, c.ValueOrBinding.GetValue());
            Assert.AreEqual(11, c.ValueOrBindingNullable?.GetValue());


            controlF1.BindingOnly = null;
            controlF2.SetCapability(c with {
                NotNullable = 31,
                Nullable = 32,
                ValueOrBinding = new(2),
                ValueOrBindingNullable = new(22)
            });

            Assert.AreEqual(2, controlF2.GetValue<int>(TestControlFallbackProps.ValueOrBindingProperty));
            Assert.AreEqual(1, controlF2.ValueOrBinding2); // this is only fallback, it shouldn't set the second property
            Assert.AreEqual(22, controlF2.ValueOrBindingNullable2); // this one is alias
            Assert.AreEqual(31, controlF2.GetValue<int>("NotNullable"));
            Assert.AreEqual(32, controlF2.GetValue<int>("Nullable"));
        }

        [TestMethod]
        public void BitMoreComplexCapability_InheritedProperties()
        {
            var control1 = new TestControlInheritedProps();
            var control2 = new TestControlInheritedProps();
            control1.Children.Add(control2);

            control1.NotNullable = 1;
            control1.Nullable = 2;

            Assert.AreEqual(1, control2.NotNullable);
            Assert.AreEqual(2, control2.Nullable);

            Assert.AreEqual(1, control1.GetCapability<BitMoreComplexCapability>().NotNullable);
            Assert.AreEqual(2, control1.GetCapability<BitMoreComplexCapability>().Nullable);
            Assert.AreEqual(1, control2.GetCapability<BitMoreComplexCapability>().NotNullable);
            Assert.AreEqual(2, control2.GetCapability<BitMoreComplexCapability>().Nullable);

            control2.SetCapability(new BitMoreComplexCapability { NotNullable = 3, Nullable = null });
            Assert.AreEqual(3, control2.NotNullable);
            Assert.AreEqual(null, control2.Nullable);
        }

        [DataTestMethod]
        [DataRow(typeof(TestControl6))]
        [DataRow(typeof(TestControlFallbackProps))]
        [DataRow(typeof(TestControlInheritedProps))]
        public void BitMoreComplexCapability_SetDefaultValue(Type controlType)
        {
            var control1 = (DotvvmBindableObject)Activator.CreateInstance(controlType);
            var capProp = DotvvmCapabilityProperty.Find(controlType, typeof(BitMoreComplexCapability));
            control1.SetValue(capProp, new BitMoreComplexCapability { NotNullable = 30, ValueOrBinding = new((int?)null) }); // default
            // XAssert.Empty(control1.Properties);
            Assert.AreEqual(30, control1.GetCapability<BitMoreComplexCapability>().NotNullable);
            Assert.AreEqual(null, control1.GetCapability<BitMoreComplexCapability>().ValueOrBinding.GetValue());

            control1.SetProperty("NotNullable", 1);
            control1.SetProperty("ValueOrBinding", 2);
            Assert.AreEqual(1, control1.GetCapability<BitMoreComplexCapability>().NotNullable);
            Assert.AreEqual(2, control1.GetCapability<BitMoreComplexCapability>().ValueOrBinding.GetValue());

            control1.SetValue(capProp, new BitMoreComplexCapability { NotNullable = 30, ValueOrBinding = new((int?)null) }); // reset defaults
            Assert.AreEqual(30, control1.GetCapability<BitMoreComplexCapability>().NotNullable);
            Assert.AreEqual(null, control1.GetCapability<BitMoreComplexCapability>().ValueOrBinding.GetValue());
        }

        [DataTestMethod]
        [DataRow(typeof(TestControl6))]
        [DataRow(typeof(TestControlFallbackProps))]
        [DataRow(typeof(TestControlInheritedProps))]
        public void BitMoreComplexCapability_SetEmptyValue(Type controlType)
        {
            var control1 = (DotvvmBindableObject)Activator.CreateInstance(controlType);
            var capProp = DotvvmCapabilityProperty.Find(controlType, typeof(BitMoreComplexCapability));

            Assert.IsFalse(control1.GetCapability<BitMoreComplexCapability>().ValueOrBindingNullable.HasValue);

            var prop = controlType == typeof(TestControlFallbackProps) ? "ValueOrBindingNullable2" : "ValueOrBindingNullable";

            control1.SetProperty(prop, null);
            XAssert.Single(control1.Properties);
            Assert.IsFalse(control1.GetCapability<BitMoreComplexCapability>().ValueOrBindingNullable.HasValue);

            control1.SetProperty(prop, 1);
            Assert.AreEqual(1, control1.GetCapability<BitMoreComplexCapability>().ValueOrBindingNullable?.GetValue());


            control1.SetValue(capProp, new BitMoreComplexCapability { ValueOrBindingNullable = null }); // removes property
            Assert.AreEqual(null, control1.GetCapability<BitMoreComplexCapability>().ValueOrBindingNullable);
            Assert.IsFalse(control1.IsPropertySet(control1.GetDotvvmProperty(prop)));

            control1.SetValue(capProp, new BitMoreComplexCapability { ValueOrBindingNullable = new(2) }); // sets new value
            Assert.AreEqual(2, control1.GetCapability<BitMoreComplexCapability>().ValueOrBindingNullable?.GetValue());
        }

        [DataTestMethod]
        [DataRow(typeof(TestControl6))]
        [DataRow(typeof(TestControlFallbackProps))]
        [DataRow(typeof(TestControlInheritedProps))]
        public void BitMoreComplexCapability_NullableValue(Type controlType)
        {
            var control1 = (DotvvmBindableObject)Activator.CreateInstance(controlType);
            var capProp = DotvvmCapabilityProperty.Find(controlType, typeof(BitMoreComplexCapability));

            Assert.IsFalse(control1.GetCapability<BitMoreComplexCapability>().Nullable.HasValue);

            control1.SetProperty("Nullable", null);
            XAssert.Single(control1.Properties);
            Assert.IsFalse(control1.GetCapability<BitMoreComplexCapability>().Nullable.HasValue);

            control1.SetProperty("Nullable", 1);
            Assert.AreEqual(1, control1.GetCapability<BitMoreComplexCapability>().Nullable);

            control1.SetValue(capProp, new BitMoreComplexCapability { Nullable = null });
            Assert.AreEqual(null, control1.GetCapability<BitMoreComplexCapability>().Nullable);
            Assert.IsTrue(control1.IsPropertySet(control1.GetDotvvmProperty("Nullable")));

            control1.SetValue(capProp, new BitMoreComplexCapability { Nullable = 2 });
            Assert.AreEqual(2, control1.GetCapability<BitMoreComplexCapability>().Nullable);
        }


        public class TestControl1:
            HtmlGenericControl,
            IObjectWithCapability<HtmlCapability>,
            IObjectWithCapability<TestCapability>
        {
            public string Something
            {
                get { return (string)GetValue(SomethingProperty); }
                set { SetValue(SomethingProperty, value); }
            }
            public static readonly DotvvmProperty SomethingProperty =
                DotvvmProperty.Register<string, TestControl1>(nameof(Something), "blbost");
        }

        public class TestControl2:
            HtmlGenericControl,
            IObjectWithCapability<TestCapability>
        {
            public string Something
            {
                get { return (string)GetValue(SomethingProperty); }
                set { SetValue(SomethingProperty, value); }
            }
            public static readonly DotvvmProperty SomethingProperty =
                DotvvmProperty.Register<string, TestControl2>(nameof(Something), "blbost2");            

            public static readonly DotvvmProperty AnotherHtml_HtmlCapabilityProperty =
                DotvvmCapabilityProperty.RegisterCapability<HtmlCapability, TestControl2>("AnotherHtml:");
        }

        public class TestControl3:
            DotvvmControl,
            IObjectWithCapability<TestNestedCapability>
        {       
            public static readonly DotvvmProperty TestNestedCapabilityProperty =
                DotvvmCapabilityProperty.RegisterCapability<TestNestedCapability, TestControl3>();
        }
        public class TestControl4:
            HtmlGenericControl,
            IObjectWithCapability<TestNestedCapability>
        {       
        }

        public class TestControl5:
            HtmlGenericControl,
            IObjectWithCapability<TestNestedCapabilityWithPrefix>
        {       
        }
        public class TestControl6:
            HtmlGenericControl,
            IObjectWithCapability<BitMoreComplexCapability>
        {       
        }
        public class TestControlFallbackProps:
            HtmlGenericControl,
            IObjectWithCapability<BitMoreComplexCapability>
        {
            // Inherited -> Can't use direct access
            public IValueBinding<string> BindingOnly
            {
                get { return (IValueBinding<string>)GetValue(BindingOnlyProperty); }
                set { SetValue(BindingOnlyProperty, value); }
            }
            public static readonly DotvvmProperty BindingOnlyProperty =
                DotvvmProperty.Register<IValueBinding<string>, TestControlFallbackProps>(nameof(BindingOnly), null, isValueInherited: true);

            // With fallback -> Can't use direct access
            public int? ValueOrBinding2
            {
                get { return (int?)GetValue(ValueOrBinding2Property); }
                set { SetValue(ValueOrBinding2Property, value); }
            }
            public static readonly DotvvmProperty ValueOrBinding2Property =
                DotvvmProperty.Register<int?, TestControlFallbackProps>(nameof(ValueOrBinding2));
            public static readonly DotvvmPropertyWithFallback ValueOrBindingProperty =
                DotvvmPropertyWithFallback.Register<int?, TestControlFallbackProps>("ValueOrBinding", fallbackProperty: ValueOrBinding2Property);


            // Alias -> should redirect automatically
            public int ValueOrBindingNullable2
            {
                get { return (int)GetValue(ValueOrBindingNullable2Property); }
                set { SetValue(ValueOrBindingNullable2Property, value); }
            }
            public static readonly DotvvmProperty ValueOrBindingNullable2Property =
                DotvvmProperty.Register<int, TestControlFallbackProps>(nameof(ValueOrBindingNullable2), defaultValue: 10);

            [PropertyAlias("ValueOrBindingNullable2")]
            public static readonly DotvvmProperty ValueOrBindingNullableProperty =
                DotvvmPropertyAlias.RegisterAlias<TestControlFallbackProps>("ValueOrBindingNullable");

            public static readonly DotvvmCapabilityProperty BitMoreComplexCapabilityProperty =
                DotvvmCapabilityProperty.RegisterCapability<BitMoreComplexCapability, TestControlFallbackProps>();
        }

        public class TestControlInheritedProps:
            HtmlGenericControl,
            IObjectWithCapability<BitMoreComplexCapability>
        {
            public int NotNullable
            {
                get { return (int)GetValue(NotNullableProperty); }
                set { SetValue(NotNullableProperty, value); }
            }
            public static readonly DotvvmProperty NotNullableProperty =
                DotvvmProperty.Register<int, TestControlInheritedProps>(nameof(NotNullable), isValueInherited: true);

            public int? Nullable
            {
                get { return (int?)GetValue(NullableProperty); }
                set { SetValue(NullableProperty, value); }
            }
            public static readonly DotvvmProperty NullableProperty =
                DotvvmProperty.Register<int?, TestControlInheritedProps>(nameof(Nullable), isValueInherited: true);

            public static readonly DotvvmCapabilityProperty BitMoreComplexCapabilityProperty =
                DotvvmCapabilityProperty.RegisterCapability<BitMoreComplexCapability, TestControlInheritedProps>();
        }

        [DotvvmControlCapability]
        public sealed record TestCapability
        {
            public string Something { get; init; } = "kokosovina";
            public string SomethingElse { get; init; } = "baf";
        }

        [DotvvmControlCapability]
        public sealed record TestNestedCapability
        {
            public string Something { get; init; } = "abc";
            public TestCapability Test { get; init; }
            public HtmlCapability Html { get; init; }
        }
        [DotvvmControlCapability]
        public sealed record TestNestedCapabilityWithPrefix
        {
            public string Something { get; init; } = "abc";
            public TestCapability Test { get; init; }

            [DotvvmControlCapability(prefix: "AnotherTest:")]
            public TestCapability AnotherTest { get; init; }

            [DotvvmControlCapability(prefix: "Item")]
            public TestCapability ItemTest { get; init; }
        }

        [DotvvmControlCapability]
        public sealed record BitMoreComplexCapability
        {
            public IValueBinding<string> BindingOnly { get; init; }
            public ValueOrBinding<int?> ValueOrBinding { get; init; }
            public ValueOrBinding<int>? ValueOrBindingNullable { get; init; }
            public int? Nullable { get; init; }
            public int NotNullable { get; init; } = 30;
        }

    }
}
