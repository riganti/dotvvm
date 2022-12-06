using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class CompositeControlPropertyRegistrationTests
    {
        // initialize properties
        IControlResolver controlResolver = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<IControlResolver>();

        [TestMethod]
        public void PropertyBindableOrHardcoded()
        {
            var valueOnlyProperty = DotvvmProperty.ResolveProperty(typeof(Control_BasicPropertyAttributes), "ValueOnlyProperty").NotNull();
            Assert.IsTrue(valueOnlyProperty.MarkupOptions.AllowHardCodedValue);
            Assert.IsFalse(valueOnlyProperty.MarkupOptions.AllowBinding);
            Assert.AreEqual(typeof(int), valueOnlyProperty.PropertyType);
            var bindingOnlyProperty = DotvvmProperty.ResolveProperty(typeof(Control_BasicPropertyAttributes), "BindingOnlyProperty").NotNull();
            Assert.IsFalse(bindingOnlyProperty.MarkupOptions.AllowHardCodedValue);
            Assert.IsTrue(bindingOnlyProperty.MarkupOptions.AllowBinding);
            Assert.AreEqual(typeof(IValueBinding<string>), bindingOnlyProperty.PropertyType);
            Assert.IsTrue(bindingOnlyProperty.IsBindingProperty);
            var property = DotvvmProperty.ResolveProperty(typeof(Control_BasicPropertyAttributes), "Property").NotNull();
            Assert.IsTrue(property.MarkupOptions.AllowHardCodedValue);
            Assert.IsTrue(property.MarkupOptions.AllowBinding);
            Assert.AreEqual(typeof(string), property.PropertyType);
            Assert.IsFalse(property.IsBindingProperty);

            Assert.IsTrue(valueOnlyProperty.MarkupOptions.Required);
            Assert.IsTrue(bindingOnlyProperty.MarkupOptions.Required);
            Assert.IsTrue(property.MarkupOptions.Required);
        }

        [TestMethod]
        public void TemplateProperties()
        {
            var template1 = DotvvmProperty.ResolveProperty(typeof(Control_BasicPropertyAttributes), "Template1").NotNull();
            Assert.AreEqual(typeof(ITemplate), template1.PropertyType);
            Assert.AreEqual(MappingMode.InnerElement, template1.MarkupOptions.MappingMode);

            var template2 = DotvvmProperty.ResolveProperty(typeof(Control_BasicPropertyAttributes), "Template2").NotNull();
            Assert.AreEqual(typeof(HtmlGenericControl), template2.PropertyType);
            Assert.AreEqual(MappingMode.InnerElement, template2.MarkupOptions.MappingMode);

            var template3 = DotvvmProperty.ResolveProperty(typeof(Control_BasicPropertyAttributes), "Template3").NotNull();
            Assert.AreEqual(typeof(IEnumerable<GridView>), template3.PropertyType);
            Assert.AreEqual(MappingMode.InnerElement, template3.MarkupOptions.MappingMode);
        }

        [TestMethod]
        public void ExplicitMappingMode()
        {
            var hiddenProperty = DotvvmProperty.ResolveProperty(typeof(Control_BasicPropertyAttributes), "HiddenProperty").NotNull();
            Assert.AreEqual(MappingMode.Exclude, hiddenProperty.MarkupOptions.MappingMode);
            Assert.IsFalse(hiddenProperty.MarkupOptions.Required);

            var anotherHiddenProperty = DotvvmProperty.ResolveProperty(typeof(Control_BasicPropertyAttributes), "AnotherHiddenProperty").NotNull();
            Assert.AreEqual(MappingMode.Exclude, anotherHiddenProperty.MarkupOptions.MappingMode);
        }

        public class Control_BasicPropertyAttributes: CompositeControl
        {
            [MarkupOptions(MappingMode = MappingMode.Exclude)]
            public string AnotherHiddenProperty
            {
                get { return (string)GetValue(AnotherHiddenPropertyProperty); }
                set { SetValue(AnotherHiddenPropertyProperty, value); }
            }
            public static readonly DotvvmProperty AnotherHiddenPropertyProperty =
                DotvvmProperty.Register<string, Control_BasicPropertyAttributes>(nameof(AnotherHiddenProperty));

            public DotvvmControl GetContents(
                int valueOnlyProperty,
                IValueBinding<string> bindingOnlyProperty,
                ValueOrBinding<string> property,
                ITemplate template1,
                // [MarkupOptions(Name = "Template2-ChangedName")]
                HtmlGenericControl template2 = null,
                IEnumerable<GridView> template3 = null,
                ITemplate template = null,
                [MarkupOptions(MappingMode = MappingMode.Exclude)]
                int hiddenProperty = 0
            )
            {
                return new PlaceHolder();
            }
        }

    }
}
