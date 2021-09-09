using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Binding;
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

            public static readonly DotvvmProperty AnotherHtmlCapabilityProperty =
                DotvvmCapabilityProperty.RegisterCapability<HtmlCapability, TestControl2>(nameof(AnotherHtmlCapabilityProperty), "AnotherHtml:");
        }

        public class TestControl3:
            DotvvmControl,
            IObjectWithCapability<TestNestedCapability>
        {       
            public static readonly DotvvmProperty TestCapabilityProperty =
                DotvvmCapabilityProperty.RegisterCapability<TestNestedCapability, TestControl3>(nameof(TestCapabilityProperty));
        }
        public class TestControl4:
            HtmlGenericControl,
            IObjectWithCapability<TestNestedCapability>
        {       
        }

        [DotvvmControlCapability]
        public sealed class TestCapability
        {
            public string Something { get; set; } = "kokosovina";
            public string SomethingElse { get; set; } = "baf";
        }

        [DotvvmControlCapability]
        public sealed class TestNestedCapability
        {
            public string Something { get; set; } = "abc";
            public TestCapability Test { get; set; }
            public HtmlCapability Html { get; set; }
        }

    }
}
